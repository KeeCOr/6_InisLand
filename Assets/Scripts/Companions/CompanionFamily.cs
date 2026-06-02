using UnityEngine;

namespace IL6
{
    public sealed class CompanionFamily : MonoBehaviour
    {
        public enum Sex { Unknown, Male, Female }

        public Sex BiologicalSex = Sex.Unknown;
        public bool IsChild;
        public int PartnerInstanceId;
        public bool EverPartnered;
        public string PartnerName;

        public bool IsAdult => !IsChild && GetComponent<VillageChildGrowth>() == null;
        public bool IsLivingAdult
        {
            get
            {
                var companion = GetComponent<Companion>();
                return IsAdult && (companion == null || !companion.IsDead);
            }
        }
        public bool HasLivingPartner => PartnerInstanceId != 0 && FindPartner() != null;

        public bool CanPairWith(CompanionFamily other)
        {
            if (other == null || other == this) return false;
            if (!IsLivingAdult || !other.IsLivingAdult) return false;
            if (EverPartnered || other.EverPartnered) return false;
            if (BiologicalSex == Sex.Unknown || other.BiologicalSex == Sex.Unknown) return false;
            return BiologicalSex != other.BiologicalSex;
        }

        public void PairWith(CompanionFamily other)
        {
            if (!CanPairWith(other)) return;
            PartnerInstanceId = other.gameObject.GetInstanceID();
            PartnerName = other.gameObject.name;
            EverPartnered = true;

            other.PartnerInstanceId = gameObject.GetInstanceID();
            other.PartnerName = gameObject.name;
            other.EverPartnered = true;
        }

        public CompanionFamily FindPartner()
        {
            if (PartnerInstanceId == 0) return null;
            var all = Object.FindObjectsByType<CompanionFamily>(FindObjectsSortMode.None);
            foreach (var f in all)
            {
                if (f == null || f.gameObject.GetInstanceID() != PartnerInstanceId) continue;
                var c = f.GetComponent<Companion>();
                if (c != null && c.IsDead) return null;
                return f;
            }
            return null;
        }

        public static Sex SexForRole(string role)
        {
            return role switch
            {
                "농부" => Sex.Female,
                "노인" => Sex.Female,
                "아이" => Sex.Unknown,
                _ => Sex.Male,
            };
        }
    }
}
