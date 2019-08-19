using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Netsphere.Common.Configuration;
using Netsphere.Database;
using Netsphere.Database.Auth;
using Netsphere.Database.Game;
using Z.EntityFramework.Plus;

namespace Netsphere.Server.Game
{
    public class ClanManager : IHostedService, IReadOnlyCollection<Clan>
    {
        private readonly ILogger _logger;
        private readonly DatabaseService _databaseService;
        private readonly IOptionsMonitor<ClanOptions> _clanOptions;
        private readonly PlayerManager _playerManager;
        private Dictionary<uint, Clan> _clans;

        public Clan this[uint id] => GetClan(id);
        public Clan this[string name] => GetClan(name);

        public ClanManager(ILogger<ClanManager> logger, DatabaseService databaseService,
            IOptionsMonitor<ClanOptions> clanOptions, PlayerManager playerManager)
        {
            _logger = logger;
            _databaseService = databaseService;
            _clanOptions = clanOptions;
            _playerManager = playerManager;

            _playerManager.PlayerConnected += OnPlayerConnected;
            _playerManager.PlayerDisconnected += OnPlayerDisconnected;
        }

        public Clan GetClan(uint id)
        {
            return _clans.GetValueOrDefault(id);
        }

        public Clan GetClan(string name)
        {
            return _clans.Values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public ClubNameCheckResult CheckClanName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return ClubNameCheckResult.CannotBeUsed;

            var clanNameRestrictions = _clanOptions.CurrentValue;
            if (name.Length < clanNameRestrictions.NameMinLength)
                return ClubNameCheckResult.TooShort;

            if (name.Length > clanNameRestrictions.NameMaxLength)
                return ClubNameCheckResult.TooLong;

            if (GetClan(name) != null)
                return ClubNameCheckResult.NotAvailable;

            return ClubNameCheckResult.Available;
        }

        public async Task<(Clan, ClanCreateError)> CreateClan(Player plr, string name, string description,
            ClubArea area, ClubActivity activity,
            string question1, string question2, string question3, string question4, string question5)
        {
            if (plr.Clan != null)
                return (null, ClanCreateError.AlreadyInClan);

            if (CheckClanName(name) != ClubNameCheckResult.Available)
                return (null, ClanCreateError.NameAlreadyExists);

            Clan clan;
            using (var db = _databaseService.Open<GameContext>())
            {
                var clanEntity = new ClanEntity
                {
                    OwnerId = (int)plr.Account.Id,
                    CreationDate = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Icon = _clanOptions.CurrentValue.DefaultIcon,
                    Name = name,
                    Description = description,
                    Area = (byte)area,
                    Activity = (byte)activity,
                    Question1 = question1,
                    Question2 = question2,
                    Question3 = question3,
                    Question4 = question4,
                    Question5 = question5
                };
                var clanMemberEntity = new ClanMemberEntity
                {
                    ClanId = clanEntity.Id,
                    PlayerId = (int)plr.Account.Id,
                    JoinDate = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    State = (byte)ClubMemberState.Joined,
                    Role = (byte)ClubRole.Master,
                    LastLoginDate = DateTimeOffset.Now.ToUnixTimeSeconds()
                };

                db.Clans.Add(clanEntity);
                clanEntity.Members.Add(clanMemberEntity);
                await db.SaveChangesAsync();

                clan = new Clan(this, clanEntity, new[]
                {
                    (
                        clanMemberEntity,
                        new AccountEntity
                        {
                            Id = (int)plr.Account.Id, Nickname = plr.Account.Nickname
                        }
                    )
                });
                _clans.Add(clan.Id, clan);
            }

            plr.Clan = clan;
            plr.ClanMember.Player = plr;
            plr.SendClubInfo();
            return (clan, ClanCreateError.None);
        }

        public Task CloseClan(uint clanId)
        {
            var clan = GetClan(clanId);
            return clan == null ? Task.CompletedTask : CloseClan(clan);
        }

        public async Task CloseClan(Clan clan)
        {
            if (clan.Members.Count() > 1)
                return;

            using (var db = _databaseService.Open<GameContext>())
            {
                db.Clans.Remove(new ClanEntity
                {
                    Id = (int)clan.Id
                });
                await db.SaveChangesAsync();
            }

            _clans.Remove(clan.Id);

            if (clan.Owner.Player != null)
            {
                clan.Owner.Player.Clan = null;
                clan.Owner.Player.SendClubInfo();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Loading clans...");

            using (var db = _databaseService.Open<GameContext>())
            using (var authDb = _databaseService.Open<AuthContext>())
            {
                var clanEntities = await db.Clans
                    .Include(x => x.Members)
                    .ToArrayAsync();

                var clans = new List<Clan>();
                foreach (var clanEntity in clanEntities)
                {
                    var memberIds = clanEntity.Members.Select(x => x.PlayerId).ToList();
                    var accounts = await authDb.Accounts
                        .Where(x => memberIds.Contains(x.Id))
                        .ToArrayAsync();

                    clans.Add(new Clan(
                        this,
                        clanEntity,
                        clanEntity.Members.Select(x => (x, accounts.First(acc => acc.Id == x.PlayerId)))
                    ));
                }

                _clans = clans.ToDictionary(x => x.Id, x => x);
            }

            _logger.Information("Loaded {Count} clans", _clans.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnPlayerConnected(object sender, PlayerEventArgs e)
        {
            var plr = e.Player;
            var member = plr.ClanMember;
            if (member != null)
            {
                member.Player = plr;
                member.LastLogin = DateTimeOffset.Now;
                using (var db = _databaseService.Open<GameContext>())
                {
                    db.ClanMembers.Where(x => x.Id == member.Id).Update(x => new ClanMemberEntity
                    {
                        LastLoginDate = member.LastLogin.ToUnixTimeSeconds()
                    });
                }

                plr.Clan.OnMemberConnected(member);
            }
        }

        private static void OnPlayerDisconnected(object sender, PlayerEventArgs e)
        {
            var plr = e.Player;
            var member = plr.ClanMember;
            if (member != null)
            {
                member.Player = null;
                plr.Clan.OnMemberDisconnected(member);
            }
        }

        #region IReadOnlyCollection
        public int Count => _clans.Count;

        public IEnumerator<Clan> GetEnumerator()
        {
            return _clans.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    public class Clan : IReadOnlyCollection<ClanMember>
    {
        private readonly Dictionary<ulong, ClanMember> _members;
        private ulong _ownerId;

        public ClanMember this[ulong id] => GetMember(id);
        public IEnumerable<ClanMember> Members => _members.Values;

        public event EventHandler<ClanMemberEventArgs> MemberConnected;
        public event EventHandler<ClanMemberEventArgs> MemberDisconnected;

        internal void OnMemberConnected(ClanMember member)
        {
            MemberConnected?.Invoke(this, new ClanMemberEventArgs(member));
        }

        internal void OnMemberDisconnected(ClanMember member)
        {
            MemberDisconnected?.Invoke(this, new ClanMemberEventArgs(member));
        }

        public ClanManager ClanManager { get; }
        public uint Id { get; }
        public DateTimeOffset CreationDate { get; }
        public string Name { get; }
        public string Icon { get; }
        public string Description { get; }
        public ClubArea Area { get; }
        public ClubActivity Activity { get; }
        public ClubClass Class { get; }
        public ClanMember Owner => GetMember(_ownerId);

        public Clan(ClanManager clanManager, ClanEntity entity, IEnumerable<(ClanMemberEntity, AccountEntity)> members)
        {
            ClanManager = clanManager;
            Id = (uint)entity.Id;
            CreationDate = DateTimeOffset.FromUnixTimeSeconds(entity.CreationDate);
            Name = entity.Name;
            Icon = entity.Icon;
            Description = entity.Description;
            Area = (ClubArea)entity.Area;
            Activity = (ClubActivity)entity.Activity;
            Class = (ClubClass)entity.Class;

            _members = new Dictionary<ulong, ClanMember>();
            _ownerId = (ulong)entity.OwnerId;

            foreach (var (memberEntity, account) in members)
                _members[(ulong)memberEntity.PlayerId] = new ClanMember(memberEntity, account.Nickname);
        }

        public ClanMember GetMember(ulong id)
        {
            return _members.GetValueOrDefault(id);
        }

        public Task Close()
        {
            return ClanManager.CloseClan(this);
        }

        #region IReadOnlyCollection
        public int Count => _members.Count;

        public IEnumerator<ClanMember> GetEnumerator()
        {
            return _members.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

    public class ClanMember
    {
        private readonly string _cachedName;

        public int Id { get; }
        public DateTimeOffset JoinDate { get; }
        public ClubMemberState State { get; internal set; }
        public ClubRole Role { get; internal set; }
        public ulong AccountId { get; }
        public string Name => Player?.Account.Nickname ?? _cachedName;
        public Player Player { get; internal set; }
        public DateTimeOffset LastLogin { get; internal set; }

        public ClanMember(ClanMemberEntity entity, string name)
        {
            Id = entity.Id;
            JoinDate = DateTimeOffset.FromUnixTimeSeconds(entity.JoinDate);
            State = (ClubMemberState)entity.State;
            Role = (ClubRole)entity.Role;
            AccountId = (ulong)entity.PlayerId;
            LastLogin = DateTimeOffset.FromUnixTimeSeconds(entity.LastLoginDate);
            _cachedName = name;
        }
    }
}
