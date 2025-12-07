using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Sentakki.Difficulty
{
    public class SentakkiPerformanceAttributes : PerformanceAttributes
    {
        [JsonProperty("base_pp")]
        public double Base_PP { get; set; }

        [JsonProperty("length_bonus")]
        public double Length_Bonus { get; set; }

        public override IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay()
        {
            foreach (var attribute in base.GetAttributesForDisplay())
            {
                yield return attribute;
            }

            yield return new PerformanceDisplayAttribute(nameof(Base_PP), "Base PP", Base_PP);
            yield return new PerformanceDisplayAttribute(nameof(Length_Bonus), "Length Bonus", Length_Bonus);
        }
    }
}
