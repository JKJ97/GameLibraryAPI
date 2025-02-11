async function addGame() {
    const fields = ["name", "genre", "price", "releaseDate"];
    const data = Object.fromEntries(fields.map(id => [id, document.getElementById(id).value.trim()]));

    // Convert release year to full date format YYYY-01-01
    data.releaseDate = `${data.releaseDate}-01-01`;

    const response = await fetch("http://localhost:5000/games", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
    });

    if (response.ok) {
        loadGames();
        fields.forEach(id => document.getElementById(id).value = "");  // Clear fields after success
    } else {
        const errorData = await response.json();
        showError(errorData.message.join("\n")); // Show all validation errors
    }
}

// Function for error messages
function showError(message) {
    let errorBox = document.getElementById("errorBox");

    if (!errorBox) {
        errorBox = document.createElement("div");
        errorBox.id = "errorBox";
        errorBox.classList.add("errorBox");
        document.body.appendChild(errorBox);
    }

    errorBox.innerText = message;
    errorBox.style.display = "block";

    setTimeout(() => {
        errorBox.style.display = "none";
    }, 3000);
}

// Load games list on page load
loadGames();
