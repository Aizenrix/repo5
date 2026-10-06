document.addEventListener('DOMContentLoaded', function () {
    const csrfToken = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');
    document.querySelectorAll('.complete-btn').forEach(btn => {
        btn.addEventListener('click', async function () {
            const taskId = this.dataset.id;
            try {
                const response = await fetch(`/Tasks?handler=Complete&id=${taskId}`, {
                    method: 'POST',
                    headers: {
                        'X-CSRF-TOKEN': csrfToken || '',
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': csrfToken || ''
                    }
                });
                if (response.ok) {
                    this.insertAdjacentHTML('afterend', ' <span>[Выполнена]</span>');
                    this.remove();
                } else {
                    alert('Ошибка: ' + response.status);
                }
            } catch (e) {
                alert('Ошибка сети');
            }
        });
    });
});
