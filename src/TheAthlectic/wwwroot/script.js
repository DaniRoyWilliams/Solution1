// Add this to a new file named script.js
document.addEventListener('DOMContentLoaded', function() {
    const heroImages = document.querySelectorAll('.hero-images img');
    let currentIndex = 0;

    function changeImage() {
        heroImages[currentIndex].classList.remove('active');
        currentIndex = (currentIndex + 1) % heroImages.length;
        heroImages[currentIndex].classList.add('active');
    }

    setInterval(changeImage, 5000); // Change image every 5 seconds
});