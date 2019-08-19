using System;
using System.Linq;
using System.Threading.Tasks;
using ExpressMapper.Extensions;
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
          IHandle<ClubJoinReqMessage>, IHandle<ClubUnjoinReqMessage>
    {
        private readonly ClanManager _clanManager;

        public ClanHandler(ClanManager clanManager)
        {
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
    }
}
