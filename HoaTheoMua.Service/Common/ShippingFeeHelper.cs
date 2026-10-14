namespace HoaTheoMua.Service.Common;

public static class ShippingFeeHelper
{
    public const decimal FeePerExtraKm = 5000m;

    // Free-ship policy on top of the carrier quote (all distances in km): free up to ahamove.free_ship km,
    // then the buyer pays FeePerExtraKm for every km past it. With no policy configured, or a distance we
    // can't trust, the buyer pays the carrier's own fee.
    public static decimal CalculateBuyerFee(double? distanceKm, double freeShippingKm, decimal carrierFee)
    {
        if (!(freeShippingKm > 0) || distanceKm is not >= 0 || !double.IsFinite(distanceKm.Value))
        {
            return carrierFee;
        }

        if (distanceKm <= freeShippingKm)
        {
            return 0;
        }

        // decimal before subtracting so 5.33 - 5 is exactly 0.33, not 0.33000000000000007.
        var extraKm = (decimal)distanceKm.Value - (decimal)freeShippingKm;
        return Math.Round(extraKm * FeePerExtraKm, 0, MidpointRounding.AwayFromZero);
    }
}
