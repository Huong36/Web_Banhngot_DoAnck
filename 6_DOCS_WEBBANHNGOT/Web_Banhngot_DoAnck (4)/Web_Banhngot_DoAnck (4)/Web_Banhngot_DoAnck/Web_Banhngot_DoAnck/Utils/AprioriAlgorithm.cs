using System;
using System.Collections.Generic;
using System.Linq;

namespace Web_Banhngot_DoAnck.Utils
{
    public class AprioriRule
    {
        public int ProductId_A { get; set; }
        public int ProductId_B { get; set; }
        public double Support { get; set; }
        public double Confidence { get; set; }
    }

    public class AprioriAlgorithm
    {
        private double _minSupport;
        private double _minConfidence;

        public AprioriAlgorithm(double minSupport, double minConfidence)
        {
            _minSupport = minSupport;
            _minConfidence = minConfidence;
        }

        public List<AprioriRule> GenerateRules(List<List<int>> transactions)
        {
            int totalTransactions = transactions.Count;
            if (totalTransactions == 0) return new List<AprioriRule>();

            // Bước 1: Tính Support cho từng sản phẩm (1-itemset)
            var itemCounts = new Dictionary<int, int>();
            foreach (var transaction in transactions)
            {
                // Xóa trùng lặp trong 1 giao dịch nếu có
                var distinctItems = transaction.Distinct().ToList();
                foreach (var item in distinctItems)
                {
                    if (!itemCounts.ContainsKey(item))
                        itemCounts[item] = 0;
                    itemCounts[item]++;
                }
            }

            // Bước 2: Tính Support cho các cặp sản phẩm (2-itemset)
            var pairCounts = new Dictionary<Tuple<int, int>, int>();
            foreach (var transaction in transactions)
            {
                var items = transaction.Distinct().OrderBy(x => x).ToList();
                for (int i = 0; i < items.Count - 1; i++)
                {
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        var pair = new Tuple<int, int>(items[i], items[j]);
                        if (!pairCounts.ContainsKey(pair))
                            pairCounts[pair] = 0;
                        pairCounts[pair]++;
                    }
                }
            }

            var rules = new List<AprioriRule>();

            // Bước 3: Lọc ra các cặp thỏa mãn MinSupport và sinh luật
            foreach (var pair in pairCounts)
            {
                double supportPair = (double)pair.Value / totalTransactions;
                if (supportPair >= _minSupport)
                {
                    int itemA = pair.Key.Item1;
                    int itemB = pair.Key.Item2;

                    double supportA = (double)itemCounts[itemA] / totalTransactions;
                    double supportB = (double)itemCounts[itemB] / totalTransactions;

                    // Sinh luật A -> B
                    double confAtoB = supportPair / supportA;
                    if (confAtoB >= _minConfidence)
                    {
                        rules.Add(new AprioriRule
                        {
                            ProductId_A = itemA,
                            ProductId_B = itemB,
                            Support = supportPair,
                            Confidence = confAtoB
                        });
                    }

                    // Sinh luật B -> A
                    double confBtoA = supportPair / supportB;
                    if (confBtoA >= _minConfidence)
                    {
                        rules.Add(new AprioriRule
                        {
                            ProductId_A = itemB,
                            ProductId_B = itemA,
                            Support = supportPair,
                            Confidence = confBtoA
                        });
                    }
                }
            }

            // Sắp xếp các luật theo Confidence giảm dần
            return rules.OrderByDescending(r => r.Confidence).ToList();
        }
    }
}
