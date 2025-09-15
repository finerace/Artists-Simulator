using Cysharp.Threading.Tasks;

namespace Game.Services.Meta
{
    public interface ICharacterCreationService
    {
        UniTask<CharacterCustomizationView> CreateCharacter(CustomizationTemplateSO template, string characterId);
        UniTask DestroyCharacter(string characterId);
        CustomizationTemplateSO GetRandomTemplate(CharacterGender gender);
    }
} 