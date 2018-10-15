using System;

namespace Origam.DA
{
    public interface ILocalizationCache: IDisposable
    {
        string GetLocalizedString(Guid elementId, string memberName,
            string defaultString, string locale);
        string GetLocalizedString(Guid elementId, string memberName,
            string defaultString);

        void Reload();
    }    
}