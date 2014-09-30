function GameState(gridCells) {
    this.gridWidth = gridCells.length;
    this.gridHeight = gridCells[0].length;
    this.canMoveUp = false;
    this.canMoveRight = false;
    this.canMoveDown = false;
    this.canMoveLeft = false;
    this.Initilize();
}

GameState.prototype.Initilize = function () {
    this.CanMoveUp = this.CanMoveUp();
    this.canMoveRight = this.CanMoveRight();
    this.canMoveDown = this.CanMoveDown();
    this.canMoveLeft = this.CanMoveLeft();
}




function GetDirection(gameManager) {
    var gridCells = [];

    for (var i = 0; i < 4; i++) {
        gridCells[i] = [];
    }

    for (var i = 0; i < 4; ++i) {
        for (var j = 0; j < 4; ++j) {
            if (gameManager.grid.cells[i] && gameManager.grid.cells[i][j]) {
                gridCells[j][i] = gameManager.grid.cells[i][j].value;
            }
            else {
                gridCells[j][i] = 0;
            }
        }
    }

    return GetDirectionForNextStep(gridCells);
}


function RunToEnd() {
    AutoRunTimer = setTimeout("RunOneStep()", 300);
}

function RunOneStep() {
    document.getElementById("btnNext").click();
    AutoRunTimer = setTimeout("RunOneStep()", 300);
}

function StopRunning() {
    clearInterval(AutoRunTimer);
    AutoRunTimer = undefined;
}
var AutoRunTimer;