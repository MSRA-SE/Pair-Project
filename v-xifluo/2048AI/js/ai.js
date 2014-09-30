function GetDirectionForNextStep(gridCells) {
    var ret = $.ajax({
        type: "GET",
        url: "api.ashx?grid=" + JSON.stringify(gridCells) + "&date=" + new Date().getUTCMilliseconds(),
        async: false,
    });
    return ret.responseText;
}






