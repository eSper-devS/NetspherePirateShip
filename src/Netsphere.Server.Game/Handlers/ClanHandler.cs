using System;
using System.Linq;
using System.Threading.Tasks;
using ExpressMapper.Extensions;
using Logging;
using Netsphere.Network;
using Netsphere.Network.Data.Club;
using Netsphere.Network.Message.Club;
using Netsphere.Server.Game.Rules;
using ProudNet;

namespace Netsphere.Server.Game.Handlers
{
    internal class ClanHandler
        : IHandle<ClubSearchReqMessage>, IHandle<ClubInfoReqMessage>, IHandle<ClubNameCheckReqMessage>,
          IHandle<ClubCreateReqMessage>, IHandle<ClubCloseReqMessage>, IHandle<ClubJoinConditionInfoReqMessage>,
          IHandle<ClubJoinReqMessage>, IHandle<ClubUnjoinReqMessage>, IHandle<ClubJoinWaiterInfoReqMessage>,
          IHandle<ClubAdminJoinCommandReqMessage>, IHandle<ClubNewJoinMemberInfoReqMessage>
    {
        private readonly ILogger _logger;
        private readonly ClanManager _clanManager;

        public ClanHandler(ILogger<ClanHandler> logger, ClanManager clanManager)
        {
            _logger = logger;
            _clanManager = clanManager;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        public async Task<bool> OnHandle(MessageContext context, ClubInfoReqMessage message)
        {
            var session = context.GetSession<Session>();
            var clan = _clanManager[message.ClubId];

            session.Send(new ClubInfoAckMessage
            {
                ClanId = clan.Id,
                ClanIcon = clan.Icon,
                ClanName = clan.Name,
                MemberCount = clan.Count(x => x.State == ClubMemberState.Joined),
                OwnerName = clan.Owner.Name,
                CreationDate = clan.CreationDate,
                Area = clan.Area,
                Activity = clan.Activity,
                Class = clan.Class,
                Description = clan.Description
            });
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        public async Task<bool> OnHandle(MessageContext context, ClubSearchReqMessage message)
        {
            var session = context.GetSession<Session>();

            // TODO Better queries, pages and sorting

            var result = _clanManager
                .Where(x => x.Name.Contains(message.Query, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Map<Clan, ClubSearchResultDto>())
                .ToArray();

            session.Send(new ClubSearchAckMessage(result));
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan), Invert = true)]
        public async Task<bool> OnHandle(MessageContext context, ClubNameCheckReqMessage message)
        {
            context.Session.Send(new ClubNameCheckAckMessage(
                _clanManager.CheckClanName(message.Name)
            ));
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan), Invert = true)]
        public async Task<bool> OnHandle(MessageContext context, ClubCreateReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;

            if (_clanManager.CheckClanName(message.Name) != ClubNameCheckResult.Available)
                session.Send(new ClubCreateAckMessage(ClubCreateResult.Failed));

            var (_, result) = await _clanManager.CreateClan(
                plr,
                message.Name, message.Description,
                message.Area, message.Activity,
                message.Question1, message.Question2, message.Question3, message.Question4, message.Question5
            );

            session.Send(result == ClanCreateError.None
                ? new ClubCreateAckMessage(ClubCreateResult.Success)
                : new ClubCreateAckMessage(ClubCreateResult.Failed));

            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan))]
        public async Task<bool> OnHandle(MessageContext context, ClubCloseReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;

            if (plr.ClanMember.Role != ClubRole.Master)
            {
                session.Send(new ClubCloseAckMessage(ClubCloseResult.MasterRequired));
                return true;
            }

            if (plr.Clan.Members.Count() > 1)
            {
                session.Send(new ClubCloseAckMessage(ClubCloseResult.ClanNotEmpty));
                return true;
            }

            await plr.Clan.Close();
            session.Send(new ClubCloseAckMessage(ClubCloseResult.Success));
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        public async Task<bool> OnHandle(MessageContext context, ClubJoinConditionInfoReqMessage message)
        {
            var session = context.GetSession<Session>();
            var clan = _clanManager[message.ClubId];

            if (clan == null)
            {
                session.Send(new Network.Message.Game.ServerResultAckMessage(ServerResult.FailedToRequestTask));
                return true;
            }

            session.Send(new ClubJoinConditionInfoAckMessage
            {
                JoinType = clan.IsPublic ? 1 : 2,
                RequiredLevel = clan.RequiredLevel,
                Question1 = clan.Question1,
                Question2 = clan.Question2,
                Question3 = clan.Question3,
                Question4 = clan.Question4,
                Question5 = clan.Question5
            });
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        public async Task<bool> OnHandle(MessageContext context, ClubJoinReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;
            var clan = _clanManager[message.ClubId];

            if (clan == null)
            {
                session.Send(new ClubJoinAckMessage(ClubJoinResult.Failed));
                return true;
            }

            var result = await clan.Join(
                plr,
                message.Answer1,
                message.Answer2,
                message.Answer3,
                message.Answer4,
                message.Answer5
            );
            session.Send(new ClubJoinAckMessage(result));
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan))]
        public async Task<bool> OnHandle(MessageContext context, ClubUnjoinReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;

            var result = await plr.Clan.Leave(plr);
            session.Send(new ClubUnjoinAckMessage(result ? ClubLeaveResult.Success : ClubLeaveResult.Failed));
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan))]
        public async Task<bool> OnHandle(MessageContext context, ClubJoinWaiterInfoReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;
            var clan = _clanManager[message.ClubId];

            if (plr.ClanMember.Role > ClubRole.Staff)
            {
                session.Send(new Network.Message.Game.ServerResultAckMessage(ServerResult.FailedToRequestTask));
                return true;
            }

            session.Send(new ClubJoinWaiterInfoAckMessage
            {
                Waiters = clan
                    .Where(x => x.State == ClubMemberState.JoinRequested)
                    .Select(x => x.Map<ClanMember, JoinWaiterInfoDto>())
                    .ToArray()
            });
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan))]
        public async Task<bool> OnHandle(MessageContext context, ClubAdminJoinCommandReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;
            var clan = plr.Clan;

            if (plr.ClanMember.Role > ClubRole.Staff)
            {
                session.Send(new ClubAdminJoinCommandAckMessage(ClubCommandResult.PermissionDenied));
                return true;
            }

            if (message.AccountIds.Length > 1)
            {
                session.Send(new ClubAdminJoinCommandAckMessage(ClubCommandResult.MemberNotFound));
                return true;
            }

            ClubCommandResult result;
            switch (message.Command)
            {
                case ClubCommand.Accept:
                    result = await clan.Approve(message.AccountIds[0]);
                    break;

                case ClubCommand.Decline:
                    result = await clan.Decline(message.AccountIds[0]);
                    break;

                case ClubCommand.Kick:
                {
                    var targetMember = clan.GetMember(message.AccountIds[0]);
                    if (targetMember.Role <= plr.ClanMember.Role)
                        result = ClubCommandResult.PermissionDenied;
                    else
                        result = await clan.Kick(message.AccountIds[0]);

                    break;
                }

                case ClubCommand.Ban:
                {
                    var targetMember = clan.GetMember(message.AccountIds[0]);
                    if (targetMember.Role <= plr.ClanMember.Role)
                        result = ClubCommandResult.PermissionDenied;
                    else
                        result = await clan.Ban(plr, message.AccountIds[0]);

                    break;
                }

                default:
                    plr.AddContextToLogger(_logger).Warning("Unknown join command={command}", message.Command);
                    result = ClubCommandResult.MemberNotFound;
                    break;
            }

            session.Send(new ClubAdminJoinCommandAckMessage(result));
            return true;
        }

        [Firewall(typeof(MustBeLoggedIn))]
        [Firewall(typeof(MustBeInClan))]
        public async Task<bool> OnHandle(MessageContext context, ClubNewJoinMemberInfoReqMessage message)
        {
            var session = context.GetSession<Session>();
            var plr = session.Player;
            var clan = plr.Clan;

            if (plr.ClanMember.Role > ClubRole.Staff)
            {
                session.Send(new Network.Message.Game.ServerResultAckMessage(ServerResult.FailedToRequestTask));
                return true;
            }

            var newMembers = clan
                .Where(x => x.State == ClubMemberState.Joined)
                .OrderByDescending(x => x.JoinDate)
                .Select(x => x.Map<ClanMember, NewMemberInfoDto>())
                .ToArray();
            session.Send(new ClubNewJoinMemberInfoAckMessage(newMembers));
            return true;
        }
    }
}
