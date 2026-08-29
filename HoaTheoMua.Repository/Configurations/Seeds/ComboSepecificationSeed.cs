using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class ComboSepecificationSeed
{
    private static readonly DateTimeOffset SeededAt =
        new(2026, 8, 24, 0, 0, 0, TimeSpan.Zero);

    public static void SeedComboSpecification(
        this EntityTypeBuilder<ComboSpecification> builder)
    {
        builder.HasData(
            // Combo 1: 1 nguyên liệu chính, 3 phụ liệu.
            Create("7a93d6e1-5b42-4f8c-a017-6e29c4b581d3",
                "5d7c48f1-2e63-4a9b-8f14-cb7291d63805",
                "0c629e7a-4d13-48b5-a967-f21d80537c94", 10, true),
            Create("c2184f75-9a36-47ed-b082-51f63d0a9c24",
                "5d7c48f1-2e63-4a9b-8f14-cb7291d63805",
                "b7523a9d-81e6-457c-93f0-6ad4182ce705", 2, false),
            Create("34e9a107-68d2-4c5f-91b7-a8032d6e4f59",
                "5d7c48f1-2e63-4a9b-8f14-cb7291d63805",
                "d80416c3-27a9-4fb1-85e6-39c7502ad184", 1, false),
            Create("9b61e2d8-3f74-49a5-8c20-e7164d93b052",
                "5d7c48f1-2e63-4a9b-8f14-cb7291d63805",
                "46fa8b21-c953-41de-a607-128d7e3c95b4", 3, false),

            // Combo 2: 2 nguyên liệu chính, 2 phụ liệu.
            Create("e5c82a49-17d6-43fb-9a35-70b4d2618e93",
                "a31f762c-9b48-4e05-bd67-285cf194a730",
                "6e19d4b7-52c8-469a-b031-f8472d5e16c3", 6, true),
            Create("1f47b6c2-8d59-4ea3-a716-95c30b2f684d",
                "a31f762c-9b48-4e05-bd67-285cf194a730",
                "83c5f920-1a74-4d6e-98b3-2f617c4a5d80", 4, true),
            Create("b03d98e6-64a1-45cf-87e2-319ab750d426",
                "a31f762c-9b48-4e05-bd67-285cf194a730",
                "b7523a9d-81e6-457c-93f0-6ad4182ce705", 2, false),
            Create("65a7c134-2e89-4bd0-b563-d918f40c7a26",
                "a31f762c-9b48-4e05-bd67-285cf194a730",
                "f24a7d18-93b6-40ce-a852-7c105e4d9b63", 1, false),

            // Combo 3: 1 nguyên liệu chính, 3 phụ liệu.
            Create("d7412b8f-30c5-46ea-9d17-84f6932b0c51",
                "e84b15d9-6c27-4fa3-91de-73b5082ac649",
                "a9682f4c-75d1-438b-90e6-3c517d8a2f49", 8, true),
            Create("28f6c913-b45e-4d72-a801-69e3f25c7b04",
                "e84b15d9-6c27-4fa3-91de-73b5082ac649",
                "d80416c3-27a9-4fb1-85e6-39c7502ad184", 2, false),
            Create("8c35e7a2-61f9-42bd-b470-15d8a3e69c24",
                "e84b15d9-6c27-4fa3-91de-73b5082ac649",
                "46fa8b21-c953-41de-a607-128d7e3c95b4", 2, false),
            Create("f9124d68-7b30-4ce5-93a1-2d846f5b7c09",
                "e84b15d9-6c27-4fa3-91de-73b5082ac649",
                "f24a7d18-93b6-40ce-a852-7c105e4d9b63", 1, false)
        );
    }

    private static ComboSpecification Create(
        string id,
        string subId,
        string materialId,
        int quantity,
        bool isCore)
    {
        return new ComboSpecification
        {
            Id = Guid.Parse(id),
            SubId = Guid.Parse(subId),
            MatId = Guid.Parse(materialId),
            Quantity = quantity,
            IsCore = isCore,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = SeededAt
        };
    }
}
