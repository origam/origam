using System.Threading.Tasks;
using System.Xml;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    public interface IManager
    {
        Task<IOrigamUser> FindByNameAsync(string name);
        Task<bool> ChangePasswordQuestionAndAnswerAsync(string userName, string password, string question, string answer);
        Task<bool> IsLockedOutAsync(string userId);
        Task<bool> GetTwoFactorEnabledAsync(string userId);
        Task<bool> SetTwoFactorEnabledAsync(string userId, bool enabled);
        Task<bool> IsEmailConfirmedAsync(string userId);
        Task<bool> UnlockUserAsync(string userName);
        Task<InternalIdentityResult> ConfirmEmailAsync(string userId);
        Task<InternalIdentityResult> ConfirmEmailAsync(string userId, string token);
        Task<InternalIdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<InternalIdentityResult> ResetPasswordFromUsernameAsync(string userName, string token, string newPassword);
        Task<InternalIdentityResult> DeleteAsync(IOrigamUser user);
        Task<InternalIdentityResult> UpdateAsync(IOrigamUser user);
        void SendNewUserToken(string userName);
        Task<InternalIdentityResult> CreateAsync(IOrigamUser user, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        Task<TokenResult> GetPasswordResetTokenFromEmailAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync1(string userId);
        Task<XmlDocument> GetPasswordAttributesAsync();
    }
}