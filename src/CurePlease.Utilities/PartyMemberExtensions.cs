using static EliteMMO.API.EliteAPI;

namespace CurePlease.Utilities
{
    public static class PartyMemberExtensions
    {
        public static uint HPLoss(this PartyMember member)
        {
            return member.CurrentHP * 100 / member.CurrentHPP - member.CurrentHP;
        }

        public static bool InParty(this PartyMember member, int partyNumber)
        {
            switch (partyNumber)
            {
                case 1:
                    return member.MemberNumber <= 5;
                case 2:
                    return member.MemberNumber > 5 && member.MemberNumber <= 11;
                case 3:
                    return member.MemberNumber > 11;
                default:
                    break;
            }

            return false;
        }
    }
}
