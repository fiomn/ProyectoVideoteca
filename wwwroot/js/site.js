﻿//send email
$("#btnSend").click(function sendEmail() {
    const email = $("#inEmail").val();
    const username = $("#inUserName").val();
    $.ajax({
        url: "/userAuthentication/sendEmail",
        type: "post",
        data: { "email": email, "username": username }, //parametros
        success: function () {
            alert('Correo enviado');
        }
    });
});

//function to have a preview image
function previewProfileImage(input) {
    if (input.files && input.files[0]) { //si se ha seleccionado al menos un archivo
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#profileImagePreview').attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

//search by name and genre
//show carousel of search results
document.addEventListener("DOMContentLoaded", function () {
    var btnSearch = document.getElementById("btnSearch");

    //search by name and genre
    function search() {
        const inputSearch = $("#inputSearch").val(); //string input
        $.ajax({
            url: "/Client/search",
            type: "get",
            data: { "inputSearch": inputSearch }, //parameters
            datatype: 'text',
            success: function (data) {
                var dataList = JSON.parse(data);

                //call the select of main view
                var selectElement = $('#mySelect');
                selectElement.empty();
                selectElement.append($('<option>', {
                    text: 'select'
                }));

                //traverse the list and add to options
                $.each(dataList, function (i, movie) {
                    selectElement.append($('<option>', {
                        value: movie.ID,
                        text: movie.TITLE
                    }));
                });
                replaceHTML(dataList);
                showCarousel();  
            }
        });
    }
    
    //call two functions for button btnSearch
    btnSearch.addEventListener("click", function () {
        search();       
    });    
});

//show results in carousel
function showCarousel() {
    document.getElementById('carRe').style.display = 'flex';
}

function replaceHTML(data) {
    var carousel = "<div class='movies-container' id='carRe'>" +
        "<div class='title-controls-container'>" +
        "<h3>Results</h3>" +
        "<div class='indicators'></div>"+
        "</div>"+
    "<div class='main-container'>" +
        "<button role='button' class='left-arrow'><i class='fas fa-angle-left'></i></button>" +
        "<div class='carousel-container'>" +
        "<div class='carousel'>";
    for (movie in data) {
        carousel += "<div class='movie'>" +
            "<div class='video-thumbnail'>" +
                "<a asp-controller='client' asp-action='detailsMovies' asp-route-TITLE='" + data[movie].TITLE + "'><img src=" + data[movie].IMG + " class='carouselImg'></a>" +
            "</div>" +
            "</div>";
    }
    carousel += "</div>" +
        "</div>" +
        "<button role='button' class='right-arrow'><i class='fas fa-angle-right'></i></button>" +
        "</div>" +
        "</div>";

    $("#carRe").replaceWith(carousel);
}