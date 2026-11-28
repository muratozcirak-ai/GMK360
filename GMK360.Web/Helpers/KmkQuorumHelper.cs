using System;
using System.Collections.Generic;
using System.Linq;
using GMK360.Core.Entities;

namespace GMK360.Web.Helpers
{
    public sealed class KmkQuorumSummary
    {
        public string QuorumType { get; set; } = "Standard";
        public string RuleLabel { get; set; } = "Genel Karar (KMK Md. 30 • >%50)";
        public int TotalUnits { get; set; }
        public int ParticipatedUnits { get; set; }
        public int ApprovedUnits { get; set; }
        public int RejectedUnits { get; set; }
        public bool UsesCustomLandShare { get; set; }
        public decimal TotalShare { get; set; }
        public decimal ApprovedShare { get; set; }
        public decimal ApprovalUnitPercent { get; set; }
        public decimal ApprovalSharePercent { get; set; }
        public int RequiredUnitCount { get; set; }
        public decimal RequiredShareThreshold { get; set; }
        public bool IsQuorumMet { get; set; }
        public bool IsRejectedByMath { get; set; }
        public bool IsTie { get; set; }
        public int MaxRejectionAllowance => Math.Max(0, TotalUnits - RequiredUnitCount);
        public string StatusText { get; set; } = string.Empty;
    }

    public static class KmkQuorumHelper
    {
        public static KmkQuorumSummary Compute(
            IEnumerable<BuildingUnit>? allUnits,
            IEnumerable<BuildingDecisionVote>? rawVotes,
            string? quorumType)
        {
            var units = (allUnits ?? Enumerable.Empty<BuildingUnit>())
                .Where(u => !(u.Category ?? "").StartsWith("OrtakAlan", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var votes = (rawVotes ?? Enumerable.Empty<BuildingDecisionVote>())
                .Where(v => !v.IsDeleted)
                .ToList();

            string qType = string.IsNullOrWhiteSpace(quorumType) ? "Standard" : quorumType.Trim();
            int totalUnits = units.Count;
            bool usesCustomShare = units.Any(u => u.LandShare > 0m);
            decimal totalShare = usesCustomShare
                ? units.Sum(u => u.LandShare)
                : totalUnits;

            var voteByUnit = votes.ToDictionary(v => v.BuildingUnitId, v => v);
            int approvedUnits = 0;
            int rejectedUnits = 0;
            decimal approvedShare = 0m;
            decimal rejectedShare = 0m;

            foreach (var u in units)
            {
                if (!voteByUnit.TryGetValue(u.Id, out var vote)) continue;
                decimal unitShare = usesCustomShare ? u.LandShare : 1m;
                if (string.Equals(vote.VoteChoice, "Kabul", StringComparison.OrdinalIgnoreCase))
                {
                    approvedUnits++;
                    approvedShare += unitShare;
                }
                else if (string.Equals(vote.VoteChoice, "Ret", StringComparison.OrdinalIgnoreCase))
                {
                    rejectedUnits++;
                    rejectedShare += unitShare;
                }
            }

            int participatedUnits = approvedUnits + rejectedUnits;
            decimal participatedShare = approvedShare + rejectedShare;

            int reqUnits;
            decimal reqShare;
            string ruleLabel;
            switch (qType)
            {
                case "Unanimous":
                    reqUnits = totalUnits;
                    reqShare = totalShare;
                    ruleLabel = "Oy Birliği (%100 — Tüm Malikler)";
                    break;
                case "ManagementPlan":
                    reqUnits = (int)Math.Ceiling(totalUnits * 0.80m);
                    reqShare = totalShare * 0.80m;
                    ruleLabel = "Yönetim Planı Değişikliği (KMK Md. 28 • 4/5 = %80)";
                    break;
                case "Innovation":
                    reqUnits = (totalUnits / 2) + 1;
                    reqShare = (totalShare / 2m) + (usesCustomShare ? 0.0001m : 1m);
                    ruleLabel = "Önemli Yenilik / Yönetici Seçimi (KMK Md. 34/42 • >%50 Sayı + Arsa Payı)";
                    break;
                case "SecondAssembly":
                    reqUnits = participatedUnits > 0 ? (participatedUnits / 2) + 1 : (totalUnits / 2) + 1;
                    reqShare = (participatedShare / 2m) + (usesCustomShare ? 0.0001m : 1m);
                    ruleLabel = "2. Genel Kurul Toplantısı (KMK Md. 30/2 • Katılanların >%50'si)";
                    break;
                default:
                    qType = "Standard";
                    reqUnits = (totalUnits / 2) + 1;
                    reqShare = (totalShare / 2m) + (usesCustomShare ? 0.0001m : 1m);
                    ruleLabel = "Genel Kurul Kararı (KMK Md. 30 • >%50 Sayı + Arsa Payı)";
                    break;
            }

            bool isMet = false;
            bool isRejected = false;
            bool isTie = false;
            if (totalUnits > 0)
            {
                isMet = qType switch
                {
                    "Unanimous" => approvedUnits == totalUnits && (!usesCustomShare || approvedShare >= totalShare),
                    "ManagementPlan" => approvedUnits * 5 >= totalUnits * 4 && approvedShare * 5m >= totalShare * 4m,
                    "SecondAssembly" => participatedUnits > 0 && approvedUnits > (participatedUnits / 2.0) && (!usesCustomShare || approvedShare * 2m > participatedShare),
                    _ => approvedUnits > (totalUnits / 2.0) && (!usesCustomShare || approvedShare * 2m > totalShare)
                };

                if (!isMet)
                {
                    if (qType == "SecondAssembly")
                    {
                        isTie = participatedUnits > 0 && approvedUnits == rejectedUnits && approvedUnits * 2 == participatedUnits;
                        isRejected = participatedUnits > 0 && (isTie || approvedUnits <= (participatedUnits / 2.0));
                    }
                    else
                    {
                        int maxPossibleUnits = totalUnits - rejectedUnits;
                        decimal maxPossibleShare = totalShare - rejectedShare;
                        bool unitImpossible = qType switch
                        {
                            "Unanimous" => rejectedUnits > 0,
                            "ManagementPlan" => maxPossibleUnits * 5 < totalUnits * 4,
                            _ => maxPossibleUnits <= (totalUnits / 2.0)
                        };
                        bool shareImpossible = usesCustomShare && qType switch
                        {
                            "Unanimous" => rejectedShare > 0m,
                            "ManagementPlan" => maxPossibleShare * 5m < totalShare * 4m,
                            _ => maxPossibleShare * 2m <= totalShare
                        };
                        isTie = participatedUnits == totalUnits && approvedUnits == rejectedUnits && approvedUnits * 2 == totalUnits;
                        isRejected = unitImpossible || shareImpossible;
                    }
                }
            }

            string statusText = isMet
                ? "Yeter Sayı Sağlandı / Yazdırılabilir"
                : isRejected
                    ? (isTie ? "Reddedildi (Eşitlik — Karar Alınamadı)" : "Reddedildi (Yeterli Oy Sağlanamadı)")
                    : $"Onay Bekliyor ({approvedUnits}/{reqUnits})";

            return new KmkQuorumSummary
            {
                QuorumType = qType,
                RuleLabel = ruleLabel,
                TotalUnits = totalUnits,
                ParticipatedUnits = participatedUnits,
                ApprovedUnits = approvedUnits,
                RejectedUnits = rejectedUnits,
                UsesCustomLandShare = usesCustomShare,
                TotalShare = totalShare,
                ApprovedShare = approvedShare,
                ApprovalUnitPercent = totalUnits > 0 ? Math.Round(approvedUnits * 100m / totalUnits, 1) : 0m,
                ApprovalSharePercent = totalShare > 0m ? Math.Round(approvedShare * 100m / totalShare, 1) : 0m,
                RequiredUnitCount = reqUnits,
                RequiredShareThreshold = Math.Round(reqShare, 2),
                IsQuorumMet = isMet,
                IsRejectedByMath = isRejected,
                IsTie = isTie,
                StatusText = statusText
            };
        }

        public static KmkQuorumSummary ComputeBoardMajority(
            IEnumerable<BuildingManager>? roster,
            IEnumerable<BoardDecisionVote>? rawVotes)
        {
            var votingAsils = (roster ?? Enumerable.Empty<BuildingManager>())
                .Where(m => m.CanVoteBoardDecisions())
                .OrderBy(m => BuildingManager.GetHierarchyLevel(m.Role))
                .ThenBy(m => m.HierarchyOrder)
                .ToList();

            var votes = (rawVotes ?? Enumerable.Empty<BoardDecisionVote>())
                .Where(v => !v.IsDeleted)
                .ToList();
            var voteByMgr = votes.GroupBy(v => v.BuildingManagerId).ToDictionary(g => g.Key, g => g.First());

            int totalAsil = votingAsils.Count;
            int approved = 0;
            int rejected = 0;
            foreach (var mgr in votingAsils)
            {
                if (!voteByMgr.TryGetValue(mgr.Id, out var v)) continue;
                if (string.Equals(v.VoteChoice, "Kabul", StringComparison.OrdinalIgnoreCase)) approved++;
                else if (string.Equals(v.VoteChoice, "Ret", StringComparison.OrdinalIgnoreCase)) rejected++;
            }

            // KMK Kesin Salt Çoğunluk: EvetSayisi > (ToplamAsilUyeSayisi / 2.0) ; Eşitlikte karar reddedilir.
            int required = totalAsil > 0 ? (totalAsil / 2) + 1 : 0;
            int maxPossibleApproved = totalAsil - rejected;
            bool met = totalAsil > 0 && approved > (totalAsil / 2.0);
            bool isTie = totalAsil > 0 && (approved + rejected) == totalAsil && approved == rejected && approved * 2 == totalAsil;
            bool isRejected = totalAsil > 0 && !met && maxPossibleApproved <= (totalAsil / 2.0);
            string statusText = met
                ? "Yeter Sayı Sağlandı / Yazdırılabilir"
                : isRejected
                    ? (isTie ? "Reddedildi (Eşitlik — Karar Alınamadı)" : "Reddedildi (Yeterli Oy Sağlanamadı)")
                    : $"Onay Bekliyor ({approved}/{required})";

            return new KmkQuorumSummary
            {
                QuorumType = "BoardMajority",
                RuleLabel = $"Yönetim Kurulu Kararı (Seviye 1-3 Asil • Salt Çoğunluk: {required}/{totalAsil}, Maks İtiraz: {Math.Max(0, totalAsil - required)})",
                TotalUnits = totalAsil,
                ParticipatedUnits = approved + rejected,
                ApprovedUnits = approved,
                RejectedUnits = rejected,
                UsesCustomLandShare = false,
                TotalShare = totalAsil,
                ApprovedShare = approved,
                ApprovalUnitPercent = totalAsil > 0 ? Math.Round(approved * 100m / totalAsil, 1) : 0m,
                ApprovalSharePercent = totalAsil > 0 ? Math.Round(approved * 100m / totalAsil, 1) : 0m,
                RequiredUnitCount = required,
                RequiredShareThreshold = required,
                IsQuorumMet = met,
                IsRejectedByMath = isRejected,
                IsTie = isTie,
                StatusText = statusText
            };
        }
    }

    public sealed class KmkBoardSlot
    {
        public int SlotOrder { get; set; }
        public int LevelNo { get; set; }
        public string SlotTitle { get; set; } = string.Empty;
        public BuildingManagerRole TargetRole { get; set; }
        public SignatureAuthorityType SuggestedSignatureType { get; set; }
        public BuildingManager? AssignedMember { get; set; }
        public bool IsReserveSeat => LevelNo == 4;
        public bool IsAuditorSeat => LevelNo == 5;
        public string StatusBadgeText => AssignedMember == null
            ? "Atama Bekliyor"
            : (AssignedMember.IsTermExpired() ? "Süresi Doldu" : "Görevde");
    }

    public sealed class KmkBoardPlan
    {
        public int TotalUnits { get; set; }
        public string TierRangeLabel { get; set; } = string.Empty;
        public string LegalStatusLabel { get; set; } = string.Empty;
        public int RecommendedAsilCount { get; set; }
        public int RecommendedYedekCount { get; set; }
        public int RecommendedDenetciCount { get; set; }
        public string RecommendedAsilNote { get; set; } = string.Empty;
        public string RecommendedYedekNote { get; set; } = string.Empty;
        public string RecommendedDenetciNote { get; set; } = string.Empty;
        public bool UsesCustomPlanCounts { get; set; }
        public int TargetAsilCount { get; set; }
        public int TargetYedekCount { get; set; }
        public int TargetDenetciCount { get; set; }
        public int ActiveAsilCount { get; set; }
        public int BoardQuorumRequired { get; set; }
        public int ActiveReserveCount { get; set; }
        public int ActiveAuditorCount { get; set; }
        public int ExpiredCount { get; set; }
        public List<KmkBoardSlot> Slots { get; set; } = new List<KmkBoardSlot>();
    }

    public static class KmkBoardRecommendationHelper
    {
        public static KmkBoardPlan BuildPlan(
            int totalUnits,
            Building? building,
            IEnumerable<BuildingManager>? activeRecords)
        {
            int recAsil, recYedek, recDenetci;
            string tierRange, legalStatus, asilNote, yedekNote, denetciNote;

            if (totalUnits <= 7)
            {
                tierRange = "1 – 7 Bağımsız Bölüm";
                legalStatus = "Yönetici seçimi isteğe bağlı (KMK Md. 34)";
                recAsil = 1; recYedek = 0; recDenetci = 0;
                asilNote = "1 Yönetici (veya malikler ortak yönetimi)";
                yedekNote = "Opsiyonel (0 veya 1)";
                denetciNote = "Opsiyonel (1 Denetçi)";
            }
            else if (totalUnits <= 24)
            {
                tierRange = "8 – 24 Bağımsız Bölüm";
                legalStatus = "Yönetim seçimi kanunen zorunlu (KMK Md. 34)";
                recAsil = 3; recYedek = 1; recDenetci = 1;
                asilNote = "1 Yönetici veya 3 Kişilik Kurul (1 Başkan + 2 Aza)";
                yedekNote = "1 – 2 Yedek";
                denetciNote = "1 Denetçi (Malik şart)";
            }
            else if (totalUnits <= 79)
            {
                tierRange = "25 – 79 Bağımsız Bölüm";
                legalStatus = "Yönetim kurulu zorunlu (KMK Md. 34)";
                recAsil = 3; recYedek = 2; recDenetci = 1;
                asilNote = "3 Kişilik Kurul (1 Başkan + 2 Aza)";
                yedekNote = "2 – 3 Yedek";
                denetciNote = "1 Denetçi veya 3'lü Denetim Kurulu";
            }
            else if (totalUnits <= 199)
            {
                tierRange = "80 – 199 Bağımsız Bölüm";
                legalStatus = "Genişletilmiş yönetim & denetim kurulu (KMK Md. 34/41)";
                recAsil = 5; recYedek = 3; recDenetci = 3;
                asilNote = "5 Kişilik Kurul (1 Başkan + 1 Bşk. Yrd. + 3 Aza)";
                yedekNote = "3 – 5 Yedek";
                denetciNote = "3 Kişilik Denetim Kurulu (Malik şart)";
            }
            else
            {
                tierRange = "200+ Bağımsız Bölüm (Toplu Yapı)";
                legalStatus = "Blok temsilcili yönetim & 3'lü denetim kurulu";
                recAsil = 7; recYedek = 5; recDenetci = 3;
                asilNote = "5 veya 7 Kişilik Kurul (Blok temsilcili)";
                yedekNote = "5 – 7 Yedek";
                denetciNote = "3 Kişilik Denetim Kurulu (Malik şart)";
            }

            bool hasCustom = building != null && (building.BoardTargetAsilCount.HasValue || building.BoardTargetYedekCount.HasValue || building.BoardTargetDenetciCount.HasValue);
            int targetAsil = Math.Max(1, building?.BoardTargetAsilCount ?? recAsil);
            int targetYedek = Math.Max(0, building?.BoardTargetYedekCount ?? recYedek);
            int targetDenetci = Math.Max(0, building?.BoardTargetDenetciCount ?? recDenetci);

            var activeList = (activeRecords ?? Enumerable.Empty<BuildingManager>())
                .Where(m => m.IsActive)
                .OrderBy(m => BuildingManager.GetHierarchyLevel(m.Role))
                .ThenBy(m => m.HierarchyOrder)
                .ThenBy(m => m.Id)
                .ToList();

            var votingAsils = activeList.Where(m => m.CanVoteBoardDecisions()).ToList();
            int activeAsilCount = votingAsils.Count;
            int quorumReq = activeAsilCount > 0 ? (activeAsilCount / 2) + 1 : 0;

            var slots = new List<KmkBoardSlot>();
            int order = 1;

            // Asil koltukları oluştur: Başkan (1) + varsa Bşk Yrd (2) + Asil Azalar (3)
            var assignedAsils = activeList.Where(m => BuildingManager.GetHierarchyLevel(m.Role) <= 3).ToList();
            var usedIds = new HashSet<int>();

            var chair = assignedAsils.FirstOrDefault(m => m.Role == BuildingManagerRole.HeadManager);
            if (chair != null) usedIds.Add(chair.Id);
            slots.Add(new KmkBoardSlot
            {
                SlotOrder = order++,
                LevelNo = 1,
                SlotTitle = targetAsil == 1 ? "Tek Yönetici / Yönetim Başkanı" : "Yönetim Kurulu Başkanı",
                TargetRole = BuildingManagerRole.HeadManager,
                SuggestedSignatureType = targetAsil == 1 ? SignatureAuthorityType.Single : SignatureAuthorityType.Joint,
                AssignedMember = chair
            });

            int remainingAsils = targetAsil - 1;
            var vice = assignedAsils.FirstOrDefault(m => m.Role == BuildingManagerRole.ViceChair && !usedIds.Contains(m.Id));
            if (remainingAsils >= 1 && (targetAsil >= 5 || vice != null))
            {
                if (vice != null) usedIds.Add(vice.Id);
                slots.Add(new KmkBoardSlot
                {
                    SlotOrder = order++,
                    LevelNo = 2,
                    SlotTitle = "Başkan Yardımcısı / Muhasip Üye",
                    TargetRole = BuildingManagerRole.ViceChair,
                    SuggestedSignatureType = SignatureAuthorityType.Joint,
                    AssignedMember = vice
                });
                remainingAsils--;
            }

            var leftAsils = assignedAsils.Where(m => !usedIds.Contains(m.Id)).ToList();
            int azaSlotsToBuild = Math.Max(remainingAsils, leftAsils.Count);
            for (int i = 0; i < azaSlotsToBuild; i++)
            {
                var mem = i < leftAsils.Count ? leftAsils[i] : null;
                if (mem != null) usedIds.Add(mem.Id);
                slots.Add(new KmkBoardSlot
                {
                    SlotOrder = order++,
                    LevelNo = 3,
                    SlotTitle = $"Asil Üye (Aza {i + 1})",
                    TargetRole = BuildingManagerRole.BoardMember,
                    SuggestedSignatureType = SignatureAuthorityType.None,
                    AssignedMember = mem
                });
            }

            var reserves = activeList.Where(m => m.Role == BuildingManagerRole.ReserveMember).ToList();
            int reserveSlots = Math.Max(targetYedek, reserves.Count);
            for (int i = 0; i < reserveSlots; i++)
            {
                slots.Add(new KmkBoardSlot
                {
                    SlotOrder = order++,
                    LevelNo = 4,
                    SlotTitle = $"Yedek Üye {i + 1}",
                    TargetRole = BuildingManagerRole.ReserveMember,
                    SuggestedSignatureType = SignatureAuthorityType.None,
                    AssignedMember = i < reserves.Count ? reserves[i] : null
                });
            }

            var auditors = activeList.Where(m => m.Role == BuildingManagerRole.Auditor).ToList();
            int auditorSlots = Math.Max(targetDenetci, auditors.Count);
            for (int i = 0; i < auditorSlots; i++)
            {
                slots.Add(new KmkBoardSlot
                {
                    SlotOrder = order++,
                    LevelNo = 5,
                    SlotTitle = auditorSlots > 1 ? $"Denetim Kurulu Üyesi {i + 1} (Malik Şart)" : "Denetçi (Kat Maliki Şart)",
                    TargetRole = BuildingManagerRole.Auditor,
                    SuggestedSignatureType = SignatureAuthorityType.None,
                    AssignedMember = i < auditors.Count ? auditors[i] : null
                });
            }

            return new KmkBoardPlan
            {
                TotalUnits = totalUnits,
                TierRangeLabel = tierRange,
                LegalStatusLabel = legalStatus,
                RecommendedAsilCount = recAsil,
                RecommendedYedekCount = recYedek,
                RecommendedDenetciCount = recDenetci,
                RecommendedAsilNote = asilNote,
                RecommendedYedekNote = yedekNote,
                RecommendedDenetciNote = denetciNote,
                UsesCustomPlanCounts = hasCustom,
                TargetAsilCount = targetAsil,
                TargetYedekCount = targetYedek,
                TargetDenetciCount = targetDenetci,
                ActiveAsilCount = activeAsilCount,
                BoardQuorumRequired = quorumReq,
                ActiveReserveCount = reserves.Count(m => m.IsEffectiveActive()),
                ActiveAuditorCount = auditors.Count(m => m.IsEffectiveActive()),
                ExpiredCount = activeList.Count(m => m.IsTermExpired()),
                Slots = slots
            };
        }
    }
}
