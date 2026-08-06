namespace BerexQms.Domain.Spc.Enums;

// Western Electric / Nelson rules
public enum ControlRuleViolation
{
    None = 0,
    Rule1_BeyondThreeSigma = 1,
    Rule2_TwoOfThreeBeyondTwoSigma = 2,
    Rule3_FourOfFiveBeyondOneSigma = 3,
    Rule4_EightConsecutiveOneSide = 4,
    Rule5_SixConsecutiveIncreasingDecreasing = 5,
    Rule6_FourteenAlternating = 6,
    Rule7_FifteenWithinOneSigma = 7,
    Rule8_EightBeyondOneSigmaBothSides = 8
}
