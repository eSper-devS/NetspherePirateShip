using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlubLib.Collections.Generic;
using Microsoft.Extensions.Options;
using Netsphere.Common.Configuration;
using Netsphere.Network.Data.GameRule;
using Netsphere.Network.Message.GameRule;
using ProudNet.Hosting.Services;

namespace Netsphere.Server.Game.GameRules
{
    public class Captain : GameRuleBase
    {
        private static readonly TimeSpan s_captainRoundTime = TimeSpan.FromMinutes(3);
        private static readonly TimeSpan s_captainWaitTime = TimeSpan.FromSeconds(8);

        private readonly CaptainOptions _options;
        private readonly Dictionary<Player, TeamId> _captains;
        private readonly ISchedulerService _schedulerService;

        private int _roundCount;

        public override GameRule GameRule => GameRule.Captain;
        public override bool HasHalfTime => false;

        public Captain(GameRuleStateMachine stateMachine, IOptions<GameOptions> gameOptions,
            IOptions<CaptainOptions> options, ISchedulerService schedulerService)
            : base(stateMachine, gameOptions)
        {
            _options = options.Value;
            _captains = new Dictionary<Player, TeamId>();
            _schedulerService = schedulerService;
            StateMachine.GameStateChanged += GameStateChanged;
        }

        private void GameStateChanged(object sender, System.EventArgs e)
        {
            switch (StateMachine.GameState)
            {
                case GameState.Playing:
                    _roundCount = 0;
                    NextRound(this, null);
                    break;

                case GameState.Result:
                    RoundEnd(this, _roundCount);
                    break;
            }
        }

        private static void RoundEnd(object context, object roundCount)
        {
            if (!(context is Captain captain))
                return;

            if ((int)roundCount != captain._roundCount)
                return;

            var alphaCaptain = captain._captains.Count(x => x.Value == TeamId.Alpha);
            var betaCaptain = captain._captains.Count(x => x.Value == TeamId.Beta);
            var winnerTeam = alphaCaptain > betaCaptain ? TeamId.Alpha : TeamId.Beta;

            captain.TeamManager[winnerTeam].Score++;
            captain.Room.Broadcast(new CaptainSubRoundWinAckMessage(3, winnerTeam));

            if ((int)roundCount >= captain.Room.Options.TimeLimit.Minutes)
            {
                captain.StateMachine.StartResult();
                return;
            }

            if (captain.StateMachine.GameState != GameState.Playing)
                return;

            var time = TimeSpan.FromSeconds(captain.Room.Options.TimeLimit.TotalSeconds);
            var diff = time - captain.StateMachine.RoundTime;
            if (diff <= s_captainWaitTime + TimeSpan.FromSeconds(2))
                return;

            captain.Room.Broadcast(new GameEventMessageAckMessage(GameEventMessage.NextRoundIn,
                (ulong)s_captainWaitTime.TotalMilliseconds, 0, 0, ""));
            captain._schedulerService.ScheduleAsync(NextRound, captain, null, s_captainWaitTime);
        }

        private static void NextRound(object context, object _)
        {
            if (!(context is Captain captain))
                return;

            if (captain.StateMachine.GameState != GameState.Playing)
                return;

            captain._captains.Clear();
            foreach (var plr in captain.Room.TeamManager.PlayersPlaying)
                captain._captains.Add(plr, plr.Team.Id);

            foreach (var plr in captain.Room.TeamManager.PlayersPlaying)
            {
                plr.Session.Send(new CaptainRoundCaptainLifeInfoAckMessage(captain._captains.Keys
                    .Select(player => new CaptainLifeDto(player.Account.Id, 500)).ToArray()));
                plr.Session.Send(new GameEventMessageAckMessage(GameEventMessage.ResetRound, 0, 0, 0, string.Empty));
                plr.Session.Send(new CaptainCurrentRoundInfoAckMessage(captain.TeamManager[TeamId.Alpha].Score,
                    captain.TeamManager[TeamId.Beta].Score));
            }

            captain._schedulerService.ScheduleAsync(RoundEnd, captain, ++captain._roundCount, s_captainRoundTime);
        }

        public override void Initialize(Room room)
        {
            base.Initialize(room);

            var playersPerTeam = Room.Options.PlayerLimit / 2;
            var spectatorsPerTeam = Room.Options.SpectatorLimit / 2;
            Room.TeamManager.Add(TeamId.Alpha, playersPerTeam, spectatorsPerTeam);
            Room.TeamManager.Add(TeamId.Beta, playersPerTeam, spectatorsPerTeam);
        }

        public override void Cleanup()
        {
            base.Cleanup();

            Room.TeamManager.Remove(TeamId.Alpha);
            Room.TeamManager.Remove(TeamId.Beta);
        }

        protected override bool CanStartGame()
        {
            if (StateMachine.GameState != GameState.Waiting)
                return false;

            // Is atleast one player ready?
            var teams = TeamManager.Values;
            return teams.Sum(team => team.Players.Count(plr => plr.IsReady)) > 0;
        }

        protected override bool HasEnoughPlayers()
        {
            // We need at least 2 players
            return TeamManager.Values.Sum(team => team.PlayersPlaying.Count()) > 1;
        }

        protected internal override Team GetWinnerTeam()
        {
            return TeamManager.Values.First();
        }

        protected override PlayerScore CreateScore(Player plr)
        {
            return new CaptainPlayerScore(_options);
        }

        protected override BriefingPlayer CreateBriefingPlayer(Player plr)
        {
            return new BriefingPlayerCaptain(plr);
        }

        protected override (uint baseGain, uint bonusGain) CalculateExperienceGained(Player plr)
        {
            var experienceRates = _options.ExperienceRates;
            var place = 1;

            var plrs = TeamManager.Players
                .Where(x => x.State == PlayerState.Waiting && x.Mode == PlayerGameMode.Normal)
                .ToArray();

            foreach (var x in plrs.OrderByDescending(x => x.Score.GetTotalScore()))
            {
                if (x == plr)
                    break;

                place++;
                if (place > 3)
                    break;
            }

            var rankingBonus = 0f;
            switch (place)
            {
                case 1:
                    rankingBonus = experienceRates.FirstPlaceBonus;
                    break;

                case 2:
                    rankingBonus = experienceRates.SecondPlaceBonus;
                    break;

                case 3:
                    rankingBonus = experienceRates.ThirdPlaceBonus;
                    break;
            }

            var experienceGained = (uint)(plr.Score.GetTotalScore() * experienceRates.ScoreFactor +
                                          rankingBonus +
                                          plrs.Length * experienceRates.PlayerCountFactor +
                                          plr.GetCurrentPlayTime().TotalMinutes * experienceRates.ExperiencePerMinute);

            return (experienceGained, 0);
        }

        protected override (uint baseGain, uint bonusGain) CalculatePENGained(Player plr)
        {
            return (0, 0);
        }

        protected internal override void OnScoreKill(ScoreContext killer, ScoreContext assist, ScoreContext target,
            AttackAttribute attackAttribute)
        {
            if (target.IsSentry)
            {
                SendScoreKill(killer, assist, target, attackAttribute);
                return;
            }

            if (_captains.TryRemove(target.Player, out _))
            {
                GetScore(killer).CaptainKills++;
                if (assist != null)
                    GetScore(assist).CaptainKillAssists++;
            }

            killer.Score.Kills++;
            if (assist != null)
                assist.Score.KillAssists++;

            target.Score.Deaths++;
            SendScoreKill(killer, assist, target, attackAttribute);

            if (_captains.All(x => x.Value != TeamId.Alpha) || _captains.All(x => x.Value != TeamId.Beta))
                RoundEnd(this, _roundCount);
        }

        protected internal override void OnScoreSuicide(Player plr)
        {
            plr.Score.Deaths++;
            plr.Score.Suicides++;
            SendScoreSuicide(plr);

            _captains.TryRemove(plr, out _);
            if (_captains.All(x => x.Value != TeamId.Alpha) || _captains.All(x => x.Value != TeamId.Beta))
                RoundEnd(this, _roundCount);
        }

        protected static CaptainPlayerScore GetScore(ScoreContext plr)
        {
            return (CaptainPlayerScore)plr.Score;
        }
<<<<<<< HEAD
=======

        private void GameStateChanged(object sender, System.EventArgs e)
        {
            switch (StateMachine.GameState)
            {
                case GameState.Playing:
                    _roundCount = 0;
                    NextRound(this, null);
                    break;

                case GameState.Result:
                    RoundEnd(this, _roundCount);
                    break;
            }
        }

        private static void RoundEnd(object context, object roundCount)
        {
            if (!(context is Captain captain))
                return;

            if ((int)roundCount != captain._roundCount)
                return;

            var alphaCaptain = captain._captains.Count(x => x.Value == TeamId.Alpha);
            var betaCaptain = captain._captains.Count(x => x.Value == TeamId.Beta);
            var winnerTeam = alphaCaptain > betaCaptain ? TeamId.Alpha : TeamId.Beta;

            captain.TeamManager[winnerTeam].Score++;
            captain.Room.Broadcast(new CaptainSubRoundWinAckMessage(3, winnerTeam));

            if ((int)roundCount >= captain.Room.Options.TimeLimit.Minutes)
            {
                captain.StateMachine.StartResult();
                return;
            }

            if (captain.StateMachine.GameState != GameState.Playing)
                return;

            var time = TimeSpan.FromSeconds(captain.Room.Options.TimeLimit.TotalSeconds);
            var diff = time - captain.StateMachine.RoundTime;
            if (diff <= s_captainWaitTime + TimeSpan.FromSeconds(2))
                return;

            captain.Room.Broadcast(new GameEventMessageAckMessage(GameEventMessage.NextRoundIn,
                (ulong)s_captainWaitTime.TotalMilliseconds, 0, 0, ""));
            captain._schedulerService.ScheduleAsync(NextRound, captain, null, s_captainWaitTime);
        }

        private static void NextRound(object context, object _)
        {
            if (!(context is Captain captain))
                return;

            if (captain.StateMachine.GameState != GameState.Playing)
                return;

            var mostPlayerCountTeam = captain.TeamManager.OrderByDescending(x => x.Value.PlayersPlaying.Count()).FirstOrDefault();
            var leastPlayerCountTeam = captain.TeamManager.FirstOrDefault(x => x.Key != mostPlayerCountTeam.Key);

            var maxHp = 100 * mostPlayerCountTeam.Value.PlayersPlaying.Count();
            var balanceHp = maxHp - (maxHp - 100 * leastPlayerCountTeam.Value.PlayersPlaying.Count());

            var lifeDtoList = new List<CaptainLifeDto>();
            foreach (var plr in mostPlayerCountTeam.Value.PlayersPlaying)
                lifeDtoList.Add(new CaptainLifeDto(plr.Account.Id, balanceHp));

            foreach (var plr in leastPlayerCountTeam.Value.PlayersPlaying)
                lifeDtoList.Add(new CaptainLifeDto(plr.Account.Id, maxHp));

            captain._captains.Clear();
            foreach (var plr in captain.Room.TeamManager.PlayersPlaying)
                captain._captains.Add(plr, plr.Team.Id);

            foreach (var plr in captain.Room.TeamManager.PlayersPlaying)
            {
                plr.Session.Send(new CaptainRoundCaptainLifeInfoAckMessage(lifeDtoList.ToArray()));
                plr.Session.Send(new GameEventMessageAckMessage(GameEventMessage.ResetRound, 0, 0, 0, string.Empty));
                plr.Session.Send(new CaptainCurrentRoundInfoAckMessage(captain.TeamManager[TeamId.Alpha].Score,
                    captain.TeamManager[TeamId.Beta].Score));
            }

            captain._schedulerService.ScheduleAsync(RoundEnd, captain, ++captain._roundCount, s_captainRoundTime);
        }
>>>>>>> Balance Captain HP
    }

    public class BriefingPlayerCaptain : BriefingPlayer
    {
        public uint NonCaptainKills { get; set; }
        public uint NonCaptainKillAssists { get; set; }
        public uint CaptainKills { get; set; }
        public uint CaptainKillAssists { get; set; }
        public uint HealPoints { get; set; }
        public uint Suicide { get; set; }
        public uint RoundsWon { get; set; }

        public BriefingPlayerCaptain(Player plr)
        {
            AccountId = plr.Account.Id;
            Experience = plr.TotalExperience;
            TeamId = plr.Team.Id;
            State = plr.State;
            Mode = plr.Mode;
            IsReady = plr.IsReady;
            TotalScore = plr.Score.GetTotalScore();

            var score = (CaptainPlayerScore)plr.Score;
            NonCaptainKills = score.NonCaptainKills;
            NonCaptainKillAssists = score.NonCaptainKillAssists;
            CaptainKills = score.CaptainKills;
            CaptainKillAssists = score.CaptainKillAssists;
            HealPoints = score.HealAssists;
            RoundsWon = score.RoundsWon;
        }

        public override void Serialize(BinaryWriter w)
        {
            base.Serialize(w);

            w.Write(CaptainKills);
            w.Write(CaptainKillAssists);
            w.Write(NonCaptainKills);
            w.Write(CaptainKillAssists);
            w.Write(HealPoints);
            w.Write(RoundsWon);
            w.Write(0);
            w.Write(0);
            w.Write(0);
            w.Write(0);
            w.Write(0);
            w.Write((byte)0);
        }
    }

    public class CaptainPlayerScore : PlayerScore
    {
        private readonly CaptainOptions _options;

        public uint NonCaptainKills { get; set; }
        public uint NonCaptainKillAssists { get; set; }
        public uint CaptainKills { get; set; }
        public uint CaptainKillAssists { get; set; }
        public uint RoundsWon { get; set; }

        public CaptainPlayerScore(CaptainOptions options)
        {
            _options = options;
        }

        public override uint GetTotalScore()
        {
            var score = (NonCaptainKills * _options.PointsPerNonCaptainKills +
                         NonCaptainKillAssists * _options.PointsPerNonCaptainKillAssists +
                         CaptainKills * _options.PointsPerCaptainKills +
                         CaptainKillAssists * _options.PointsPerCaptainKillAssists +
                         RoundsWon * _options.PointsPerRoundWins);

            score -= Suicides * _options.PointsPerSuicide;
            if (score < 0)
                score = 0;

            return (uint)score;
        }

        public override void Reset()
        {
            base.Reset();
            NonCaptainKills = 0;
            NonCaptainKillAssists = 0;
            CaptainKills = 0;
            CaptainKillAssists = 0;
            RoundsWon = 0;
        }
    }
}
