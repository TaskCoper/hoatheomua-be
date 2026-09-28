using HoaTheoMua.Repository.Entity;
using static HoaTheoMua.Service.Flower.MappingFlower;
namespace HoaTheoMua.Service.Flower;

// Immutable job input; never deserialize a live EF entity graph from caller input.
public record PreparedFlower(Guid FlowerId, Guid UserId, string Name, ProductSnapshot Product,
    HoaTheoMua.Repository.Entity.FlowerReference Bundle, HoaTheoMua.Repository.Entity.FlowerReference Mockup, PromptSnapshot Prompt, FlowerMetadata Metadata,
    string Operation, Guid? SourceFlowerId, Guid SessionId, string PromptSource, Guid BaseId);
