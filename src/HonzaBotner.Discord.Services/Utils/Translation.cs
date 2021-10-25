using System;
using System.Collections.Generic;
using HonzaBotner.Discord.Utils;

namespace HonzaBotner.Discord.Services.Utils
{
    public class Translation : ITranslation
    {
        private readonly Dictionary<string, Dictionary<ITranslation.Language, string>> Texts = new()
        {
            {
                "RolesSuccessfullyDeleted",
                new()
                {
                    { ITranslation.Language.Czech, "Role byly úspěšně odstraněny." },
                    { ITranslation.Language.English, "Roles have been successfully deleted." }
                }
            },
            {
                "UserNotVerified", new()
                {
                    {
                        ITranslation.Language.Czech, "Ahoj, ještě nejsi ověřený!\n" +
                                                     "1) Pro ověření a přidělení rolí dle UserMap klikni na tlačítko dole. ✅\n" +
                                                     "2) Následně znovu klikni na tlačítko pro přidání zaměstnaneckých rolí. 👑"
                    },
                    {
                        ITranslation.Language.English, "Hi, you are not verified yet!\n" +
                                                       "1) Click the button below to verify and assign roles according to UserMap. ✅\n" +
                                                       "2) Then click the button to add employee roles again. 👑"
                    }
                }
            },
            {
                "UserAlreadyVerified", new()
                {
                    {
                        ITranslation.Language.Czech, "Ahoj, už jsi ověřený.\n" +
                                                     "Pro aktualizaci zaměstnaneckých rolí klikni na tlačítko."
                    },
                    {
                        ITranslation.Language.English, "Hi, you are already verified.\n" +
                                                       "Click the button to update employee roles."
                    }
                }
            },
            {
                "VerifyRolesButton",
                new() { { ITranslation.Language.Czech, "Ověřit se" }, { ITranslation.Language.English, "Verify" } }
            },
            {
                "UpdateStaffRolesButton",
                new()
                {
                    { ITranslation.Language.Czech, "Aktualizovat role zaměstnance" },
                    { ITranslation.Language.English, "Update staff roles" }
                }
            },
            {
                "VerifyStaffRolesButton",
                new()
                {
                    { ITranslation.Language.Czech, "Ověřit role zaměstnance" },
                    { ITranslation.Language.English, "Verify staff roles" }
                }
            },
            {
                "RemoveRolesButton",
                new()
                    {
                        { ITranslation.Language.Czech, "Odebrat role" },
                        { ITranslation.Language.English, "Remove roles" }
                    }
            },
            {
                "VerifyStaff",
                new()
                {
                    { ITranslation.Language.Czech, "Ahoj, pro ověření rolí zaměstnance klikni na tlačítko." },
                    { ITranslation.Language.English, "Hi, click the button to verify the staff roles." }
                }
            },
            {
                "AlreadyVerified",
                new()
                {
                    { ITranslation.Language.Czech, "Ahoj, už jsi ověřený.\nPro aktualizaci rolí klikni na tlačítko." },
                    {
                        ITranslation.Language.English,
                        "Hi, you are already verified.\nClick the button to update the roles."
                    }
                }
            },
            {
                "Verify",
                new()
                {
                    { ITranslation.Language.Czech, "Ahoj, pro ověření a přidělení rolí klikni na tlačítko." },
                    { ITranslation.Language.English, "Hi, click the button to verify and assign roles." }
                }
            },
            {
                "UpdateRolesButton",
                new()
                {
                    { ITranslation.Language.Czech, "Aktualizovat role" },
                    { ITranslation.Language.English, "Update roles" }
                }
            }
        };

        private ITranslation.Language _language = ITranslation.Language.English;

        public void SetLanguage(ITranslation.Language language)
        {
            _language = language;
        }

        public string GetText(string key, ITranslation.Language? language)
        {
            if (!Texts.ContainsKey(key))
            {
                throw new ArgumentException($"Provided key '{key}' is not a valid translation key.");
            }

            return Texts[key][language ?? _language];
        }
    }
}
