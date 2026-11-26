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
                }
            }

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
                default:
                    qType = "Standard";
                    reqUnits = (totalUnits / 2) + 1;
                    reqShare = (totalShare / 2m) + (usesCustomShare ? 0.0001m : 1m);
                    ruleLabel = "Genel Kurul Kararı (KMK Md. 30 • >%50 Çoğunluk)";
                    break;
            }

            bool isMet = false;
            if (totalUnits > 0)
            {
                isMet = qType switch
                {
                    "Unanimous" => approvedUnits == totalUnits && (!usesCustomShare || approvedShare >= totalShare),
                    "ManagementPlan" => approvedUnits * 5 >= totalUnits * 4 && approvedShare * 5m >= totalShare * 4m,
                    _ => approvedUnits * 2 > totalUnits && (!usesCustomShare || approvedShare * 2m > totalShare)
                };
            }

            return new KmkQuorumSummary
            {
                QuorumType = qType,
                RuleLabel = ruleLabel,
                TotalUnits = totalUnits,
                ParticipatedUnits = approvedUnits + rejectedUnits,
                ApprovedUnits = approvedUnits,
                RejectedUnits = rejectedUnits,
                UsesCustomLandShare = usesCustomShare,
                TotalShare = totalShare,
                ApprovedShare = approvedShare,
                ApprovalUnitPercent = totalUnits > 0 ? Math.Round(approvedUnits * 100m / totalUnits, 1) : 0m,
                ApprovalSharePercent = totalShare > 0m ? Math.Round(approvedShare * 100m / totalShare, 1) : 0m,
                RequiredUnitCount = reqUnits,
                RequiredShareThreshold = Math.Round(reqShare, 2),
                IsQuorumMet = isMet
            };
        }
    }
}
