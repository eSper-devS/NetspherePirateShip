using System;
using System.Linq;
using System.Threading.Tasks;
using ExpressMapper.Extensions;
using Netsphere.Network.Data.Club;
using Netsphere.Network.Message.Club;
using Netsphere.Server.Game.Rules;
using ProudNet;

namespace Netsphere.Server.Game.Handlers
{
    internal class ClanHandler
        : IHandle<ClubSearchReqMessage>, IHandle<ClubInfoReqMessage>
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
            var plr = session.Player;
            var clan = plr.Clan;

            session.Send(new ClubInfoAckMessage
            {
                ClanId = clan.Id,
                ClanIcon = clan.Icon,
                ClanName = clan.Name,
                MemberCount = clan.Count,
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
        [Firewall(typeof(MustBeInClan), Invert = true)]
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
    }
}
