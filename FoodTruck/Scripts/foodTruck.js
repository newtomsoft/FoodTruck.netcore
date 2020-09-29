function readURL(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#ImagePreview')
                .attr('src', e.target.result)
                .width(300);
        };
        reader.readAsDataURL(input.files[0]);
    }
}

function MiseAJourDateFin(date, dateFin)
{
    var maintenant = new Date();
    var dans1an = new Date();
    dans1an.setFullYear(dans1an.getFullYear() + 1);
    var dateOk = new Date(date);
    if (maintenant <= dateOk && dateOk <= dans1an && !(document.getElementById(dateFin).value >= date))
    {
        document.getElementById(dateFin).value = date;
    }
}

function MiseAJourDateDebut(date, dateDebut)
{
    var maintenant = new Date();
    var dans1an = new Date();
    dans1an.setFullYear(dans1an.getFullYear() + 1);
    var dateOk = new Date(date);
    if (maintenant <= dateOk && dateOk <= dans1an && !(document.getElementById(dateDebut).value <= date))
    {
        document.getElementById(dateDebut).value = date;
    }
}

function MiseAJourHeureFin(heure, heureFin, dateDebut, dateFin)
{
    if (dateDebut == dateFin && !(document.getElementById(heureFin).value >= heure))
    {
        document.getElementById(heureFin).value = heure;
    }
}

function MiseAJourHeureDebut(heure, heureDebut, dateDebut, dateFin)
{
    if (dateDebut == dateFin && !(document.getElementById(heureDebut).value <= heure))
    {
        document.getElementById(heureDebut).value = heure;
    }
}

$('#popup1').popup();



//Use keyup to capture user input & mouse up to catch when user is changing the value with the arrows
$('.trailing-decimal-input').on('keyup mouseup', function (e) {

    // on keyup check for backspace & delete, to allow user to clear the input as required
    var key = e.keyCode || e.charCode;
    if (key == 8 || key == 46) {
        return false;
    };

    // get the current input value
    let correctValue = $(this).val().toString();

    //if there is no decimal places add trailing zeros
    if (correctValue.indexOf('.') === -1) {
        correctValue += '.00';
    }

    else {

        //if there is only one number after the decimal add a trailing zero
        if (correctValue.toString().split(".")[1].length === 1) {
            correctValue += '0'
        }

        //if there is more than 2 decimal places round backdown to 2
        if (correctValue.toString().split(".")[1].length > 2) {
            correctValue = parseFloat($(this).val()).toFixed(2).toString();
        }
    }

    //update the value of the input with our conditions
    $(this).val(correctValue);
});