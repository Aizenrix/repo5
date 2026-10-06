using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TodoManager.Pages;

public class ErrorModel : PageModel
{
    public int Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    public void OnGet(int code)
    {
        Code = code;

        (Title, Message, Icon) = code switch
        {
            400 => ("Неверный запрос", "Сервер не смог обработать запрос. Возможно, данные в форме некорректны или отсутствует токен безопасности.", "⚠️"),
            401 => ("Требуется авторизация", "Для доступа к этой странице необходимо войти в систему.", "🔐"),
            403 => ("Доступ запрещён", "У вас недостаточно прав для просмотра этой страницы.", "🚫"),
            404 => ("Страница не найдена", "Запрашиваемая страница не существует или была удалена.", "🔍"),
            500 => ("Ошибка сервера", "На сервере произошла внутренняя ошибка. Попробуйте позже.", "💥"),
            _ => ("Что-то пошло не так", $"Произошла ошибка с кодом {code}.", "❌")
        };
    }
}
