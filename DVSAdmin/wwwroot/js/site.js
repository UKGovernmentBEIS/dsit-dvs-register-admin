// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {
    const container = document.querySelector('.dashboard-summary');
    if (!container) return;

    function selectCard(card) {
        container.querySelectorAll('[data-card]').forEach(el => {
            el.classList.remove('dashboard-summary__card--emphasised');
            el.setAttribute('aria-selected', 'false');
        });
        card.classList.add('dashboard-summary__card--emphasised');
        card.setAttribute('aria-selected', 'true');
    }

    container.addEventListener('click', function (e) {
        const card = e.target.closest('[data-card]');
        if (!card) return;

        e.preventDefault();

        selectCard(card);
    });

    container.addEventListener('keydown', function (e) {
        const card = e.target.closest('[data-card]');
        if (!card) return;
        if (e.key === ' ' || e.key === 'Enter') {
            e.preventDefault();
            selectCard(card);
        }
    });
});
