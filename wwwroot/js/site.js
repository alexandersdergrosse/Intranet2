// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {

    const submenuButtons =
        document.querySelectorAll("[data-submenu-toggle]");

    submenuButtons.forEach(function (button) {

        button.addEventListener("click", function (event) {

            event.preventDefault();
            event.stopPropagation();

            const submenu =
                button.closest(".dropdown-submenu");

            // Andere geöffnete Untermenüs schließen
            document
                .querySelectorAll(".dropdown-submenu.show")
                .forEach(function (otherSubmenu) {

                    if (otherSubmenu !== submenu) {
                        otherSubmenu.classList.remove("show");
                    }
                });

            // Aktuelles Untermenü öffnen/schließen
            submenu.classList.toggle("show");
        });

    });


    // Untermenüs schließen, wenn Kreutzträger+ geschlossen wird
    document
        .querySelectorAll(".dropdown")
        .forEach(function (dropdown) {

            dropdown.addEventListener(
                "hidden.bs.dropdown",
                function () {

                    dropdown
                        .querySelectorAll(".dropdown-submenu.show")
                        .forEach(function (submenu) {
                            submenu.classList.remove("show");
                        });

                });
        });

});