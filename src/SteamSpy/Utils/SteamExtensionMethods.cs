using Steamworks;

namespace SteamSpy.Utils
{
    internal static class SteamExtensionMethods
    {
        public static string ToErrorStringMessage(this EResult self)
        {
            switch (self)
            {
                case EResult.k_EResultOK: return "ОК";
                case EResult.k_EResultFail: return "Ошибка";
                case EResult.k_EResultNoConnection: return "Нет подключения";
                case EResult.k_EResultInvalidPassword: return "Неверный пароль";
                case EResult.k_EResultLoggedInElsewhere: return "Аккаунт уже авторизован в другом месте";
                case EResult.k_EResultInvalidProtocolVer: return "Неверная версия протокола";
                case EResult.k_EResultInvalidParam: return "Неверные параметры";
                case EResult.k_EResultFileNotFound: return "Фаил не найден";
                case EResult.k_EResultBusy: return "Занято";
                case EResult.k_EResultInvalidState: return "Неверное состояние";
                case EResult.k_EResultInvalidName: return "Неверное имя";
                case EResult.k_EResultInvalidEmail: return "Неверный e-mail";
                case EResult.k_EResultDuplicateName: return "Имя продублировано";
                case EResult.k_EResultAccessDenied: return "Доступ запрещен";
                case EResult.k_EResultTimeout: return "Время истекло";
                case EResult.k_EResultBanned: return "Бан";
                case EResult.k_EResultAccountNotFound: return "Аккаунт не найден";
                case EResult.k_EResultInvalidSteamID: return "Неверный Steam id";
                case EResult.k_EResultServiceUnavailable: return "Сервис недоступен";
                case EResult.k_EResultNotLoggedOn: return "Аккаунт неавторизован";
                case EResult.k_EResultPending: return "Ожидание";
                case EResult.k_EResultEncryptionFailure: return "Ошибка шифрования";
                case EResult.k_EResultInsufficientPrivilege: return "Недостаточно прав";
                case EResult.k_EResultLimitExceeded: return "Превышен лимит";
                case EResult.k_EResultRevoked: return "Отменен";
                case EResult.k_EResultExpired: return "Устарело";
                case EResult.k_EResultAlreadyRedeemed: return "Уже погашен";
                case EResult.k_EResultDuplicateRequest: return "Дубликат запроса";
                case EResult.k_EResultAlreadyOwned: return "Уже является владельцем";
                case EResult.k_EResultIPNotFound: return "Не найдено";
                case EResult.k_EResultPersistFailed: return "Ошибка сохранения";
                case EResult.k_EResultLockingFailed: return "Ошибка блокировки";
                case EResult.k_EResultLogonSessionReplaced: return "Сессия перемещена";
                case EResult.k_EResultConnectFailed: return "Ошибка подключения";
                case EResult.k_EResultHandshakeFailed: return "Ошибка установления подключения";
                case EResult.k_EResultIOFailure: return "Ошибка ввода";
                case EResult.k_EResultRemoteDisconnect: return "Удаленное подключение завершено";
                case EResult.k_EResultShoppingCartNotFound: return "Карта покупки не найдена";
                case EResult.k_EResultBlocked: return "Блокировано";
                case EResult.k_EResultIgnored: return "Игнорировано";
                case EResult.k_EResultNoMatch: return "Нет совпадений";
                case EResult.k_EResultAccountDisabled: return "Аккаунт отключен";
                case EResult.k_EResultServiceReadOnly: return "Сервис только для чтения";
                case EResult.k_EResultAccountNotFeatured: return "Данная возможность недоступна";
                case EResult.k_EResultAdministratorOK: return "ОК";
                case EResult.k_EResultContentVersion: return "Версия контента";
                case EResult.k_EResultTryAnotherCM: return "Попробуй другой СМ";
                case EResult.k_EResultPasswordRequiredToKickSession: return "Пароль требует перезапуск сессии";
                case EResult.k_EResultAlreadyLoggedInElsewhere: return "Аккаунт уже авторизован в другом месте";
                case EResult.k_EResultSuspended: return "Приостановлено";
                case EResult.k_EResultCancelled: return "Отменено";
                case EResult.k_EResultDataCorruption: return "Данные повреждены";
                case EResult.k_EResultDiskFull: return "Диск переполнен";
                case EResult.k_EResultRemoteCallFailed: return "Удаленный запрос завершился с ошибкой";
                case EResult.k_EResultPasswordUnset: return "Пароль неустановлен";
                case EResult.k_EResultExternalAccountUnlinked: return "Внешний аккаунт отсоединен";
                case EResult.k_EResultPSNTicketInvalid: return "PSN Билет неверен";
                case EResult.k_EResultExternalAccountAlreadyLinked: return "Внешний аккаунт уже присоединен";
                case EResult.k_EResultRemoteFileConflict: return "Конфликт удаленного файла";
                case EResult.k_EResultIllegalPassword: return "Неверный пароль";
                case EResult.k_EResultSameAsPreviousValue: return "Совпадает с предыдущим";
                case EResult.k_EResultAccountLogonDenied: return "Авторизация отклонена";
                case EResult.k_EResultCannotUseOldPassword: return "Невозможно использовать старый пароль";
                case EResult.k_EResultInvalidLoginAuthCode: return "Неверный код авторизации";
                case EResult.k_EResultAccountLogonDeniedNoMail: return "Авторизация отклонена. Нет почты";
                case EResult.k_EResultHardwareNotCapableOfIPT: return "Аппаратная часть не имеет поддержки IPT";
                case EResult.k_EResultIPTInitError: return "Ошибка инифиализации IPT";
                case EResult.k_EResultParentalControlRestricted: return "Запрещено родительским контролем";
                case EResult.k_EResultFacebookQueryError: return "Ошибка запроса Facebook";
                case EResult.k_EResultExpiredLoginAuthCode: return "Код авторизации устарел";
                case EResult.k_EResultIPLoginRestrictionFailed: return "Авторизаци по IP ограничена";
                case EResult.k_EResultAccountLockedDown: return "Аккаунт заблокирован";
                case EResult.k_EResultAccountLogonDeniedVerifiedEmailRequired: return "Авторизация отклонена. Требуется подтверждение e-mail";
                case EResult.k_EResultNoMatchingURL: return "URL Адрес не совпадает";
                case EResult.k_EResultBadResponse: return "Неверный ответ";
                case EResult.k_EResultRequirePasswordReEntry: return "Требуется ввести пароль заново";
                case EResult.k_EResultValueOutOfRange: return "Значение вышло за границы массива";
                case EResult.k_EResultUnexpectedError: return "Непредвиденная ошибка";
                case EResult.k_EResultDisabled: return "Отключен";
                case EResult.k_EResultInvalidCEGSubmission: return "Неверное представление CEG";
                case EResult.k_EResultRestrictedDevice: return "Устройство ограничено";
                case EResult.k_EResultRegionLocked: return "Регион запрещен";
                case EResult.k_EResultRateLimitExceeded: return "Превышен лимит ставки";
                case EResult.k_EResultAccountLoginDeniedNeedTwoFactor: return "Авторизация отклонена. Требуется двухфакторная авторизация";
                case EResult.k_EResultItemDeleted: return "Элемент удален";
                case EResult.k_EResultAccountLoginDeniedThrottle: return "Авторизация отклонена";
                case EResult.k_EResultTwoFactorCodeMismatch: return "Ошибка двухфакторной авторизации";
                case EResult.k_EResultTwoFactorActivationCodeMismatch: return "Неверный код авторизации";
                case EResult.k_EResultAccountAssociatedToMultiplePartners: return "Аккаунт связан с несколькими партнера";
                case EResult.k_EResultNotModified: return "Не модифицируется";
                case EResult.k_EResultNoMobileDevice: return "Мобильное устройство отсутствует";
                case EResult.k_EResultTimeNotSynced: return "Время несинхронизировано";
                case EResult.k_EResultSmsCodeFailed: return "Неверный смс-код";
                case EResult.k_EResultAccountLimitExceeded: return "Превышен лимит авторизации";
                case EResult.k_EResultAccountActivityLimitExceeded: return "Превышен лимит активности аккаунта";
                case EResult.k_EResultPhoneActivityLimitExceeded: return "Привышен лимит активности телефона";
                case EResult.k_EResultRefundToWallet: return "Возврат денег на кошелек";
                case EResult.k_EResultEmailSendFailure: return "Ошибка повторной отправки e-mail";
                case EResult.k_EResultNotSettled: return "Не установлено";
                case EResult.k_EResultNeedCaptcha: return "Требуется капча";
                case EResult.k_EResultGSLTDenied: return "GSLT отклонен";
                case EResult.k_EResultGSOwnerDenied: return "Владелец GS отклонен";
                case EResult.k_EResultInvalidItemType: return "Неверный тип элемента";
                default: return "";
            }
        }
    }
}
