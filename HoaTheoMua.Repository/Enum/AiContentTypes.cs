namespace HoaTheoMua.Repository.Enum;

public enum SystemPromptType
{
    Flower,
    Card,
    Post
}

public enum PlatformType
{
    Zalo,
    Facebook,
    Instagram
}

public enum CardTemplateType
{
    Ai,
    Handmade
}

public enum GeneratedCardType
{
    Ai,
    Handmade
}

public enum ClientHistoryType
{
    Flower,
    Card,
    HandmadeCard,
    Post
}

public enum CardFormType
{
    GoMay,
    Calligraphy
}

public enum AspectRatioType { Square1x1, Portrait4x5, Portrait3x4, Story9x16, Landscape16x9 }
public enum PlatformPostStatus { Draft, Generating, Generated, Failed, Ready, Scheduled, Publishing, Published, PublishFailed }
public enum ContentSource { Ai, Manual }
public enum ImageStatus { Succeeded, Failed }
