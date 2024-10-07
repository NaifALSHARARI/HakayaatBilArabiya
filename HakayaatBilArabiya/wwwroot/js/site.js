document.querySelectorAll('.character').forEach(function (character) {
    character.addEventListener('mouseover', function () {
        character.classList.add('animate__animated', 'animate__bounce');
    });

    character.addEventListener('mouseout', function () {
        character.classList.remove('animate__animated', 'animate__bounce');
    });
});


window.onload = function () {
    document.querySelectorAll('.story-content').forEach(function (text) {
        text.style.opacity = 0;
        setTimeout(function () {
            text.style.opacity = 1;
            text.style.transition = 'opacity 1.5s';
        }, 500);
    });
};

document.querySelectorAll('button').forEach(function (button) {
    button.addEventListener('click', function () {
        this.style.backgroundColor = '#ff6600';
    });
});
