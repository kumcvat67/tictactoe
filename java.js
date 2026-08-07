let currentID = null;

const cells = document.querySelectorAll(".grid");
const start = document.getElementById("start");

start.addEventListener('click', startGame);

cells.forEach(function(cell) {
    cell.addEventListener("click", async function() {
        if (!currentID) {
            alert("Спочатку натисніть Start!");
            return;
        }

        const x = parseInt(cell.dataset.x);
        const y = parseInt(cell.dataset.y);

        const res = await step(x, y, currentID);

        if (res.ok) {
            renderBoard(res.data.board);
            if(res.data.status==="XWin"){
                document.body.style.backgroundColor="green";
            } else if(res.data.status==="OWin"){
                document.body.style.backgroundColor="red";
            }
        }
    });
});

async function step(x, y, id) {
    const request = JSON.stringify({ "x": x, "y": y, "id": id });
    const response = await fetch("http://localhost:5161/step", {
        method: "POST", 
        headers: { 'Content-Type': 'application/json' }, 
        body: request
    });
    
    const data = await response.json();
    return { ok: response.ok, data: data };
}

async function startGame() {
    const response = await fetch("http://localhost:5161/start", { method: "POST" });
    
    const data = await response.json();

    currentID = data.gameID || data.GameID; 
    console.log("Гра розпочата, ID:", currentID);
}

function renderBoard(board) {
    const cells = document.querySelectorAll('.grid');

    cells.forEach(cell => {
        const x = parseInt(cell.dataset.x);
        const y = parseInt(cell.dataset.y);
        const value = board[x][y];

        if (value === 1) {
            cell.textContent = "X";
        } else if (value === 2) {
            cell.textContent = "O";
        } else {
            cell.textContent = "";
        }
    });
}