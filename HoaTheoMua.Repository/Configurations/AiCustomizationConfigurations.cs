using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations;

public class SystemPromptConfiguration : IEntityTypeConfiguration<SystemPrompt>
{
    public void Configure(EntityTypeBuilder<SystemPrompt> builder)
    {
        builder.ToTable("SystemPrompt");
        builder.HasKey(systemPrompt => systemPrompt.Id);
        builder.Property(systemPrompt => systemPrompt.Id).HasColumnName("Id");
        builder.Property(systemPrompt => systemPrompt.Type)
            .HasColumnName("Type")
            .HasColumnType("\"SystemPromptType\"")
            .IsRequired();
        builder.Property(systemPrompt => systemPrompt.Content).HasColumnName("Content").HasColumnType("text").IsRequired();
        builder.HasIndex(systemPrompt => systemPrompt.Type).IsUnique();
    }
}

public class PlatformStandardConfiguration : IEntityTypeConfiguration<PlatformStandard>
{
    public void Configure(EntityTypeBuilder<PlatformStandard> builder)
    {
        builder.ToTable("PlatformStandard");
        builder.HasKey(platformStandard => platformStandard.Id);
        builder.Property(platformStandard => platformStandard.Id).HasColumnName("Id");
        builder.Property(platformStandard => platformStandard.Platform)
            .HasColumnName("Platform")
            .HasColumnType("\"PlatformType\"")
            .IsRequired();
        builder.Property(platformStandard => platformStandard.Description).HasColumnName("Description").HasColumnType("text").IsRequired();
        builder.HasIndex(platformStandard => platformStandard.Platform);
    }
}

public class CardTemplateConfiguration : IEntityTypeConfiguration<CardTemplate>
{
    public void Configure(EntityTypeBuilder<CardTemplate> builder)
    {
        builder.ToTable("CardTemplate", table =>
            table.HasCheckConstraint("CK_card_templates_template_type", "\"TemplateType\" IN ('ai', 'handmade')"));
        builder.HasKey(cardTemplate => cardTemplate.Id);
        builder.Property(cardTemplate => cardTemplate.Id).HasColumnName("Id");
        builder.Property(cardTemplate => cardTemplate.Name).HasColumnName("Name").HasMaxLength(200).IsRequired();
        builder.Property(cardTemplate => cardTemplate.Description).HasColumnName("Description").HasColumnType("text");
        builder.Property(cardTemplate => cardTemplate.ImageUrl).HasColumnName("ImageUrl").HasMaxLength(2048).IsRequired();
        builder.Property(cardTemplate => cardTemplate.TemplateType)
            .HasColumnName("TemplateType")
            .HasConversion(
                value => value == CardTemplateType.Ai ? "ai" : "handmade",
                value => value == "ai" ? CardTemplateType.Ai : CardTemplateType.Handmade)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(cardTemplate => cardTemplate.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
        builder.Property(cardTemplate => cardTemplate.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        builder.Property(cardTemplate => cardTemplate.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("now()");
        builder.Property(cardTemplate => cardTemplate.UpdatedAt).HasColumnName("UpdatedAt");

        builder.HasIndex(cardTemplate => new
            {
                cardTemplate.TemplateType,
                cardTemplate.IsActive,
                cardTemplate.CreatedAt
            })
            .IsDescending(false, false, true)
            .HasFilter("\"IsDeleted\" = false");
    }
}

public class BasePostConfiguration : IEntityTypeConfiguration<BasePost>
{
    public void Configure(EntityTypeBuilder<BasePost> builder)
    {
        builder.ToTable("BasePost");
        builder.HasKey(basePost => basePost.Id);
        builder.Property(basePost => basePost.Id).HasColumnName("Id");
        builder.Property(basePost => basePost.Content).HasColumnName("Content").HasColumnType("text").IsRequired();
        builder.Property(basePost => basePost.ImageUrl).HasColumnName("ImageUrl").HasMaxLength(2048).IsRequired();
    }
}

public class GeneratedPostConfiguration : IEntityTypeConfiguration<GeneratedPost>
{
    public void Configure(EntityTypeBuilder<GeneratedPost> builder)
    {
        builder.ToTable("GeneratedPost");
        builder.HasKey(generatedPost => generatedPost.Id);
        builder.Property(generatedPost => generatedPost.Id).HasColumnName("Id");
        builder.Property(generatedPost => generatedPost.Content).HasColumnName("Content").HasColumnType("text").IsRequired();
        builder.Property(generatedPost => generatedPost.ImageUrl).HasColumnName("ImageUrl").HasMaxLength(2048).IsRequired();
        builder.Property(generatedPost => generatedPost.UserId).HasColumnName("UserId");
        builder.Property(generatedPost => generatedPost.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("now()");
        builder.HasIndex(generatedPost => new { generatedPost.UserId, generatedPost.CreatedAt }).IsDescending(false, true);
    }
}

public class GeneratedFlowerConfiguration : IEntityTypeConfiguration<GeneratedFlower>
{
    public void Configure(EntityTypeBuilder<GeneratedFlower> builder)
    {
        builder.ToTable("GeneratedFlower");
        builder.HasKey(generatedFlower => generatedFlower.Id);
        builder.Property(generatedFlower => generatedFlower.Id).HasColumnName("Id");
        builder.Property(generatedFlower => generatedFlower.ImageUrl).HasColumnName("ImageUrl").HasMaxLength(2048).IsRequired();
        builder.Property(generatedFlower => generatedFlower.UserId).HasColumnName("UserId");
        builder.Property(generatedFlower => generatedFlower.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("now()");
        builder.HasIndex(generatedFlower => new { generatedFlower.UserId, generatedFlower.CreatedAt }).IsDescending(false, true);
    }
}

public class GeneratedCardConfiguration : IEntityTypeConfiguration<GeneratedCard>
{
    public void Configure(EntityTypeBuilder<GeneratedCard> builder)
    {
        builder.ToTable("GeneratedCard", table =>
        {
            table.HasCheckConstraint("CK_generated_cards_card_type", "\"CardType\" IN ('ai', 'handmade')");
            table.HasCheckConstraint(
                "CK_generated_cards_non_negative_values",
                "\"SizeWidth\" >= 0 AND \"SizeHeight\" >= 0 AND \"SizeBasePrice\" >= 0 AND \"SizeMaxWords\" >= 0 AND \"WordCount\" >= 0 AND \"BasePrice\" >= 0 AND \"ExtraPrice\" >= 0 AND \"TotalPrice\" >= 0");
            table.HasCheckConstraint(
                "CK_generated_cards_handmade",
                "\"CardType\" <> 'handmade' OR (\"FormType\" = 'calligraphy' AND \"AttachedImageUrl\" IS NULL AND \"RawImage\" IS NULL AND \"BasePrice\" = 0 AND \"TotalPrice\" = \"ExtraPrice\")");
            table.HasCheckConstraint(
                "CK_generated_cards_ai_images",
                "\"CardType\" <> 'ai' OR (\"RawImage\" IS NOT NULL AND \"ImageUrl\" IS NOT NULL)");
        });

        builder.HasKey(generatedCard => generatedCard.Id);
        builder.Property(generatedCard => generatedCard.Id).HasColumnName("Id");
        builder.Property(generatedCard => generatedCard.Content).HasColumnName("Content").HasColumnType("text").IsRequired();
        builder.Property(generatedCard => generatedCard.ImageUrl).HasColumnName("ImageUrl").HasMaxLength(2048).IsRequired();
        builder.Property(generatedCard => generatedCard.RawImage).HasColumnName("RawImage").HasMaxLength(2048);
        builder.Property(generatedCard => generatedCard.UserId).HasColumnName("UserId");
        builder.Property(generatedCard => generatedCard.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("now()");
        builder.Property(generatedCard => generatedCard.CardType)
            .HasColumnName("CardType")
            .HasConversion(
                value => value == GeneratedCardType.Ai ? "ai" : "handmade",
                value => value == "ai" ? GeneratedCardType.Ai : GeneratedCardType.Handmade)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(generatedCard => generatedCard.FormType)
            .HasColumnName("FormType")
            .HasColumnType("\"CardFormType\"")
            .IsRequired();
        builder.Property(generatedCard => generatedCard.MessageContent).HasColumnName("MessageContent").HasColumnType("text").IsRequired();
        builder.Property(generatedCard => generatedCard.AttachedImageUrl).HasColumnName("AttachedImageUrl").HasMaxLength(2048);
        builder.Property(generatedCard => generatedCard.SizeName).HasColumnName("SizeName").HasMaxLength(100).IsRequired();
        builder.Property(generatedCard => generatedCard.SizeWidth).HasColumnName("SizeWidth").HasPrecision(10, 2);
        builder.Property(generatedCard => generatedCard.SizeHeight).HasColumnName("SizeHeight").HasPrecision(10, 2);
        builder.Property(generatedCard => generatedCard.SizeBasePrice).HasColumnName("SizeBasePrice").HasPrecision(18, 2);
        builder.Property(generatedCard => generatedCard.SizeMaxWords).HasColumnName("SizeMaxWords");
        builder.Property(generatedCard => generatedCard.WordCount).HasColumnName("WordCount");
        builder.Property(generatedCard => generatedCard.WordConfigSnapshot).HasColumnName("WordConfigSnapshot").HasColumnType("jsonb");
        builder.Property(generatedCard => generatedCard.BasePrice).HasColumnName("BasePrice").HasPrecision(18, 2);
        builder.Property(generatedCard => generatedCard.ExtraPrice).HasColumnName("ExtraPrice").HasPrecision(18, 2);
        builder.Property(generatedCard => generatedCard.TotalPrice).HasColumnName("TotalPrice").HasPrecision(18, 2);

        builder.HasIndex(generatedCard => new { generatedCard.UserId, generatedCard.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(generatedCard => new { generatedCard.CardType, generatedCard.CreatedAt }).IsDescending(false, true);
    }
}

public class PostHistoryConfiguration : IEntityTypeConfiguration<PostHistory>
{
    public void Configure(EntityTypeBuilder<PostHistory> builder)
    {
        builder.ToTable("PostHistory");
        builder.HasKey(postHistory => postHistory.Id);
        builder.Property(postHistory => postHistory.Id).HasColumnName("Id");
        builder.Property(postHistory => postHistory.Input).HasColumnName("Input").HasColumnType("jsonb").IsRequired();
        builder.Property(postHistory => postHistory.BaseId).HasColumnName("BaseId");
        builder.Property(postHistory => postHistory.OutputId).HasColumnName("OutputId");
        builder.Property(postHistory => postHistory.SystemPromptId).HasColumnName("SystemPromptId");
        builder.Property(postHistory => postHistory.Metadata).HasColumnName("Metadata").HasColumnType("jsonb");
        builder.Property(postHistory => postHistory.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("now()");
        builder.HasOne<SystemPrompt>()
            .WithMany()
            .HasForeignKey(postHistory => postHistory.SystemPromptId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(postHistory => postHistory.OutputId).IsUnique();
        builder.HasIndex(postHistory => new { postHistory.BaseId, postHistory.CreatedAt }).IsDescending(false, true);
    }
}

public class MockupConfiguration : IEntityTypeConfiguration<Mockup>
{
    public void Configure(EntityTypeBuilder<Mockup> builder)
    {
        builder.ToTable("Mockup");
        builder.HasKey(mockup => mockup.Id);
        builder.Property(mockup => mockup.Id).HasColumnName("Id");
        builder.Property(mockup => mockup.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
        builder.Property(mockup => mockup.Description).HasColumnName("Description").HasMaxLength(200);
        builder.Property(mockup => mockup.ImageUrl).HasColumnName("ImageUrl").HasMaxLength(2048).IsRequired();
        builder.Property(mockup => mockup.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
        builder.Property(mockup => mockup.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        builder.Property(mockup => mockup.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("now()");
        builder.Property(mockup => mockup.UpdatedAt).HasColumnName("UpdatedAt");
        builder.HasIndex(mockup => new { mockup.IsActive, mockup.CreatedAt })
            .IsDescending(false, true)
            .HasFilter("\"IsDeleted\" = false");
    }
}

public class ClientHistoryConfiguration : IEntityTypeConfiguration<ClientHistory>
{
    public void Configure(EntityTypeBuilder<ClientHistory> builder)
    {
        builder.ToTable("ClientHistory");
        builder.HasKey(clientHistory => clientHistory.Id);
        builder.Property(clientHistory => clientHistory.Id).HasColumnName("Id");
        builder.Property(clientHistory => clientHistory.UserId).HasColumnName("UserId");
        builder.Property(clientHistory => clientHistory.Type)
            .HasColumnName("Type")
            .HasColumnType("\"ClientHistoryType\"")
            .IsRequired();
        builder.Property(clientHistory => clientHistory.SystemForm).HasColumnName("SystemForm").HasColumnType("jsonb");
        builder.Property(clientHistory => clientHistory.Metadata).HasColumnName("Metadata").HasColumnType("jsonb").IsRequired();
        builder.Property(clientHistory => clientHistory.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("now()");
        builder.Property(clientHistory => clientHistory.RootId).HasColumnName("RootId").IsRequired();
        builder.Property(clientHistory => clientHistory.BaseId).HasColumnName("BaseId");
        builder.Property(clientHistory => clientHistory.OutputId).HasColumnName("OutputId");

        // BaseId/OutputId are logical polymorphic references, so they intentionally have no FK.
        builder.HasIndex(clientHistory => new { clientHistory.Type, clientHistory.OutputId }).IsUnique();
        builder.HasIndex(clientHistory => new { clientHistory.UserId, clientHistory.Type, clientHistory.CreatedAt }).IsDescending(false, false, true);
        builder.HasIndex(clientHistory => new { clientHistory.BaseId, clientHistory.Type, clientHistory.CreatedAt }).IsDescending(false, false, true);
    }
}
