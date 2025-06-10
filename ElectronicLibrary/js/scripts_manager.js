document.addEventListener("DOMContentLoaded", function () {

  const btnBooks = document.getElementById('btnBooks');
  const btnGenre = document.getElementById('btnGenre');
  const booksMenu = document.getElementById('booksMenu');
  const genreMenu = document.getElementById('genreMenu');

  function toggleMenu(button, menu) {
    const rect = button.getBoundingClientRect();
    const offsetY = 9; // на 10px нижче
    const offsetX = -19; // на 20px лівіше
    menu.style.top = rect.bottom + window.scrollY  + offsetY + "px";
    menu.style.left = rect.left + window.scrollX + offsetX + "px";
    menu.classList.toggle('hidden');
  }

  btnBooks.addEventListener('click', (e) => {
    genreMenu.classList.add('hidden');
    toggleMenu(btnBooks, booksMenu);
  });

  btnGenre.addEventListener('click', (e) => {
    booksMenu.classList.add('hidden');
    toggleMenu(btnGenre, genreMenu);
  });

  document.addEventListener('click', function (e) {
    if (!e.target.closest('.submenu') && !e.target.closest('.btn-header')) {
      booksMenu.classList.add('hidden');
      genreMenu.classList.add('hidden');
    }
  });



  // === Фільтр за типом книги ===
const typeFilterContainer = document.getElementById('typeFilterContainer');
fetch('http://localhost:5278/api/booktypes')
  .then(res => res.json())
  .then(data => {
    typeFilterContainer.innerHTML = '';
    data.forEach(type => {
      const label = document.createElement('label');
      label.style.display = 'block';

      const checkbox = document.createElement('input');
      checkbox.type = 'checkbox';
      checkbox.name = 'bookType';
      checkbox.value = type.id;

      label.appendChild(checkbox);
      label.appendChild(document.createTextNode(' ' + type.name));
      typeFilterContainer.appendChild(label);
    });
  });

// === Фільтр за жанром ===
const genreFilterContainer = document.getElementById('genreFilterContainer');

function loadGenresFilter() {
  fetch('http://localhost:5278/api/genres', {
    headers: { "Authorization": "Bearer " + localStorage.getItem("token") }
  })
  .then(res => res.json())
  .then(data => {
    genreFilterContainer.innerHTML = '';
    data.forEach(genre => {
      const label = document.createElement('label');
      label.style.display = 'block';

      const checkbox = document.createElement('input');
      checkbox.type = 'checkbox';
      checkbox.name = 'genre';
      checkbox.value = genre.id;

      label.appendChild(checkbox);
      label.appendChild(document.createTextNode(' ' + genre.name));
      genreFilterContainer.appendChild(label);
    });
  })
  .catch(err => {
    console.error('Помилка завантаження жанрів:', err);
  });
}

// Завантажуємо жанри при першому завантаженні сторінки
loadGenresFilter();

// === Фільтр за видавництвом ===
const publisherFilterContainer = document.getElementById('publisherFilterContainer');
fetch('http://localhost:5278/api/books')
  .then(res => res.json())
  .then(data => {
    publisherFilterContainer.innerHTML = '';
    const uniquePublishers = [...new Set(data.map(book => book.publisher).filter(Boolean))];
    uniquePublishers.forEach(publisher => {
      const label = document.createElement('label');
      label.style.display = 'block';

      const checkbox = document.createElement('input');
      checkbox.type = 'checkbox';
      checkbox.name = 'publisher';
      checkbox.value = publisher;

      label.appendChild(checkbox);
      label.appendChild(document.createTextNode(' ' + publisher));
      publisherFilterContainer.appendChild(label);
    });
  });

// === Кнопка "Скинути фільтри" ===
const resetBtn = document.getElementById('resetFiltersBtn');
resetBtn.addEventListener('click', () => {
  const checkboxes = document.querySelectorAll(
    '#typeFilterContainer input[type="checkbox"], ' +
    '#genreFilterContainer input[type="checkbox"], ' +
    '#publisherFilterContainer input[type="checkbox"]'
  );
  checkboxes.forEach(cb => cb.checked = false);
  loadBooks(document.getElementById('searchInput').value); // Перезавантажити після скидання
});

// === Пошук та відображення книг ===
const productWrapper = document.querySelector('.product-wrapper');

function loadBooks(searchText = '') {
  fetch('http://localhost:5278/api/books')
    .then(response => response.json())
    .then(data => {
      productWrapper.innerHTML = '';
      const normalizedSearch = searchText.trim().toLowerCase();

      // === Отримуємо вибрані значення фільтрів ===
      const selectedTypes = Array.from(document.querySelectorAll('input[name="bookType"]:checked')).map(cb => cb.value);
      const selectedGenres = Array.from(document.querySelectorAll('input[name="genre"]:checked')).map(cb => cb.value);
      const selectedPublishers = Array.from(document.querySelectorAll('input[name="publisher"]:checked')).map(cb => cb.value.toLowerCase());
     


      // === Фільтрація ===
      const filteredBooks = data.filter(book => {
        const matchesSearch =
          (book.title?.toLowerCase().includes(normalizedSearch) || '') ||
          (book.author?.toLowerCase().includes(normalizedSearch) || '') ||
          (book.publisher?.toLowerCase().includes(normalizedSearch) || '');

        const matchesType = selectedTypes.length === 0 || 
    (book.availableTypeIds && book.availableTypeIds.some(typeId => selectedTypes.includes(typeId.toString())));

        const matchesGenre = selectedGenres.length === 0 || selectedGenres.includes(book.genreId?.toString());
        const matchesPublisher = selectedPublishers.length === 0 || selectedPublishers.includes(book.publisher?.toLowerCase());

        return matchesSearch && matchesType && matchesGenre && matchesPublisher;
      });

      // === Виведення карток книг ===
      filteredBooks.forEach(book => {
        const card = document.createElement('div');
        card.className = 'book-card';
        card.style.border = '1px solid #ccc';
        card.style.padding = '10px';
        card.style.margin = '10px';
        card.style.display = 'inline-block';
        card.style.width = '200px';

        const img = document.createElement('img');
        //img.src = `image/books/${book.title}.jpg`; 
          img.src = "file:///D:/Coding/DigitalLibrary/API/image/books/" + book.title + ".jpg";
        img.alt = book.title;
        img.style.width = '100%';
        img.style.height = 'auto';
        img.onerror = () => { img.src = 'image/books/default.jpg'; };

        const title = document.createElement('h4');
        title.textContent = book.title;

        const author = document.createElement('p');
        author.textContent = 'Автор: ' + book.author;

        const genre = document.createElement('p');
        genre.textContent = 'Жанр: ' + book.genreName;

        const detailsBtn = document.createElement('button');
        detailsBtn.textContent = 'Детальніше';
        detailsBtn.classList.add('btn1');
        detailsBtn.onclick = () => showBookModal(book);

        card.appendChild(img);
        card.appendChild(title);
        card.appendChild(author);
        card.appendChild(genre);
        card.appendChild(detailsBtn);

        productWrapper.appendChild(card);
      });

      if (filteredBooks.length === 0) {
         const message_wrap = document.createElement('div');
        message_wrap.className = "message-wrap";
        productWrapper.appendChild(message_wrap);
        const message = document.createElement('div');
        message.className = "book-none";
        message.textContent = 'Книг не знайдено.';
        message_wrap.appendChild(message);
      }
    })
    .catch(error => console.error('Помилка при завантаженні книг:', error));
}


// === Обробка пошуку ===
const searchBtn = document.getElementById('searchBtn');
const searchInput = document.getElementById('searchInput');

searchBtn.addEventListener('click', () => {
  const query = searchInput.value;
  loadBooks(query);
});

searchInput.addEventListener('keydown', e => {
  if (e.key === 'Enter') {
    const query = searchInput.value;
    loadBooks(query);
  }
});

// === Модальне вікно ===
const bookModal = document.getElementById('bookModal');
const bookModalClose = document.getElementById('bookModalClose');

function showBookModal(book) {
  document.getElementById('modalImage').src = "file:///D:/Coding/DigitalLibrary/API/image/books/" + book.title + ".jpg";
  //document.getElementById('modalImage').src = `image/books/${book.title}.jpg`;
  document.getElementById('modalTitle').textContent = book.title;
  document.getElementById('modalAuthor').textContent = book.author;
  document.getElementById('modalPublisher').textContent = book.publisher;
  document.getElementById('modalYear').textContent = book.publicationYear;
  document.getElementById('modalGenre').textContent = book.genreName;
  document.getElementById('modalDescription').textContent = book.description || 'Опис відсутній';
  bookModal.style.display = 'flex';
}

bookModalClose.onclick = () => {
  bookModal.style.display = 'none';
};

window.onclick = event => {
  if (event.target === bookModal) {
    bookModal.style.display = 'none';
  }
};

// === Стартове завантаження ===
loadBooks();


document.addEventListener('change', event => {
  if (event.target.matches('input[name="bookType"], input[name="genre"], input[name="publisher"]')) {
    loadBooks(searchInput.value);
  }
});





document.getElementById('btnAddBook').addEventListener('click', function () {
  document.getElementById('addBookModal').style.display = 'flex';
  loadBookGenres();
  loadBookTypes();
});

document.getElementById('closeAddBookModal').addEventListener('click', function () {
  document.getElementById('addBookModal').style.display = 'none';
});

const token = "qwerty";

// Завантажити жанри
function loadBookGenres() {
  
  fetch('http://localhost:5278/api/genres', {
    headers: {
      "Authorization": `Bearer ${token}`
    }
  })
    .then(response => response.json())
    .then(data => {
      const genreSelect = document.getElementById('bookGenre');
      genreSelect.innerHTML = '';
      data.forEach(genre => {
        const option = document.createElement('option');
        option.value = genre.id;
        option.textContent = genre.name;
        genreSelect.appendChild(option);
      });
    })
    .catch(err => {
      console.error('Помилка завантаження жанрів:', err);
    });
}

function loadBookTypes() {
  fetch('http://localhost:5278/api/booktypes')
    .then(response => response.json())
    .then(data => {
      const container = document.getElementById('bookTypesContainer');
      container.innerHTML = '';
      data.forEach(type => {
        const label = document.createElement('label');
        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.value = type.id;
        label.appendChild(checkbox);
        label.appendChild(document.createTextNode(' ' + type.name));
        container.appendChild(label);
        // container.appendChild(document.createElement('br'));
      });
    });
}



document.getElementById("addBookForm").addEventListener("submit", async function(e) {
  e.preventDefault();

  const token = "qwerty";
  const bookData = {
    title: document.getElementById("bookTitle").value,
    author: document.getElementById("bookAuthor").value,
    publisher: document.getElementById("bookPublisher").value,
    publicationYear: parseInt(document.getElementById("bookYear").value),
    genreId: parseInt(document.getElementById("bookGenre").value),
    availableTypeIds: Array.from(document.querySelectorAll("#bookTypesContainer input:checked")).map(cb => parseInt(cb.value)),
    copyCount: parseInt(document.getElementById("copyCount").value),
    description: document.getElementById("bookDescription").value
  };

  // Надсилаємо книгу
  try {
    const res = await fetch("http://localhost:5278/api/books", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
      },
      body: JSON.stringify(bookData)
    });

    if (!res.ok) throw new Error("Помилка при додаванні книги");

    const result = await res.text();
    const bookTitle = result.title;

    // Тепер надсилаємо фото (якщо вибране)
    const photoInput = document.getElementById("bookImage");
    if (photoInput.files.length > 0) {
      const formData = new FormData();
      formData.append("file", photoInput.files[0]);
      formData.append("bookName", bookTitle);

      const imgRes = await fetch("http://localhost:5278/api/ImageUpload/books", {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${token}`
        },
        body: formData
      });

      if (!imgRes.ok) throw new Error("Помилка при завантаженні фото");
    }

    alert("Книга успішно додана!");
    document.getElementById("addBookModal").style.display = "none";
    loadBooks(document.getElementById('searchInput').value);

  } catch (error) {
    console.error(error);
    alert("Сталася помилка");
  }
});




// Відкрити модалку
document.getElementById("btnEditBook").addEventListener("click", function () {
  document.getElementById("editBookModal").style.display = "flex";
  loadBookList();
  loadGenres();
  loadTypes();
});

// Закрити модалку
document.getElementById("closeEditModal").addEventListener("click", function () {
  document.getElementById("editBookModal").style.display = "none";
});

// Після вибору книги – завантажити її дані
document.getElementById("editBookId").addEventListener("change", function () {
  const id = this.value;
  if (!id) return;

  fetch("http://localhost:5278/api/books/" + id, {
    headers: {
      "Authorization": "Bearer " + token
    }
  })
  .then(res => res.json())
  .then(book => {
    document.getElementById("editBookTitle").value = book.title || '';
    document.getElementById("editBookAuthor").value = book.author || '';
    document.getElementById("editBookPublisher").value = book.publisher || '';
    document.getElementById("editBookYear").value = book.publicationYear || '';
    document.getElementById("editBookGenre").value = book.genreId || '';
    document.getElementById("editCopyCount").value = book.initialCopies || '';
    document.getElementById("editBookDescription").value = book.description || '';

    // Встановити галочки типів
    const checkboxes = document.querySelectorAll("#editBookTypesContainer input");
    checkboxes.forEach(cb => {
      cb.checked = book.availableTypeIds.includes(parseInt(cb.value));
    });
  });
});

// Відправка змін
document.getElementById("editBookForm").addEventListener("submit", function (e) {
  e.preventDefault();

  const id = document.getElementById("editBookId").value;
  if (!id) return alert("Оберіть книгу");

  const data = {};
  const get = id => document.getElementById(id).value.trim();

  if (get("editBookTitle")) data.title = get("editBookTitle");
  if (get("editBookAuthor")) data.author = get("editBookAuthor");
  if (get("editBookPublisher")) data.publisher = get("editBookPublisher");
  if (get("editBookYear")) data.publicationYear = parseInt(get("editBookYear"));
  if (get("editBookGenre")) data.genreId = parseInt(get("editBookGenre"));
  if (get("editCopyCount")) data.copyCount = parseInt(get("editCopyCount"));
  if (get("editBookDescription")) data.description = get("editBookDescription");

  const checkedTypes = Array.from(document.querySelectorAll("#editBookTypesContainer input:checked")).map(cb => parseInt(cb.value));
  if (checkedTypes.length) data.availableTypeIds = checkedTypes;

  fetch("http://localhost:5278/api/books/" + id, {
  method: "PUT",
  headers: {
    "Content-Type": "application/json",
    "Authorization": "Bearer " + token
  },
  body: JSON.stringify(data)
})
.then(res => {
  if (!res.ok) {
    return res.text().then(text => {
      throw new Error(text || "Помилка сервера");
    });
  }
  // якщо сервер не повертає JSON, просто повертаємо null
  return res.text().then(text => {
    try {
      return text ? JSON.parse(text) : null;
    } catch {
      return null;
    }
  });
})
.then(() => {
  alert("Книгу оновлено!");
  document.getElementById("editBookModal").style.display = "none";
  loadBooks(); 
})
.catch(err => {
  console.error(err);
  alert("Сталася помилка: " + err.message);
});

});

// Завантажити список книг
function loadBookList() {
  fetch("http://localhost:5278/api/books", {
    headers: { "Authorization": "Bearer " + token }
  })
  .then(res => res.json())
  .then(books => {
    const select = document.getElementById("editBookId");
    select.innerHTML = '<option value="">-- Оберіть --</option>';
    books.forEach(book => {
      const opt = document.createElement("option");
      opt.value = book.id;
      opt.textContent = book.title;
      select.appendChild(opt);
    });
  });
}

// Завантажити жанри
function loadGenres() {
  fetch("http://localhost:5278/api/genres", {
    headers: { "Authorization": "Bearer " + token }
  })
  .then(res => res.json())
  .then(genres => {
    const select = document.getElementById("editBookGenre");
    select.innerHTML = '<option value="">-- Оберіть жанр --</option>';
    genres.forEach(g => {
      const opt = document.createElement("option");
      opt.value = g.id;
      opt.textContent = g.name;
      select.appendChild(opt);
    });
  });
}

// Завантажити типи
function loadTypes() {
  fetch("http://localhost:5278/api/booktypes", {
    headers: { "Authorization": "Bearer " + token }
  })
  .then(res => res.json())
  .then(types => {
    const container = document.getElementById("editBookTypesContainer");
    container.innerHTML = '';
    types.forEach(t => {
      const label = document.createElement("label");
      label.innerHTML = `<input type="checkbox" value="${t.id}"> ${t.name}`;
      container.appendChild(label);
      container.appendChild(document.createElement("br"));
    });
  });
}




//const token = localStorage.getItem("token"); // отримуємо токен з localStorage

const deleteModal = document.getElementById("deleteBookModal");
const deleteBookListBody = document.querySelector("#deleteBookList tbody");
const openDeleteModalBtn = document.getElementById("btnDeleteBook");
const closeDeleteModal = document.getElementById("closeDeleteModal");

// Відкриття модалки
openDeleteModalBtn.addEventListener("click", () => {
  loadBooksForDelete();
  deleteModal.style.display = "flex";
   document.body.style.overflow = "hidden";
});

const modalContent = document.querySelector("#deleteBookModal .modal-content");
modalContent.style.maxHeight = "100vh";
modalContent.style.overflowY = "auto";

// Закриття модалки
closeDeleteModal.addEventListener("click", () => {
  deleteModal.style.display = "none";
   document.body.style.overflow = "";
});

// Завантажити список книг для видалення
function loadBooksForDelete() {
  fetch("http://localhost:5278/api/books", {
    headers: {
      "Authorization": "Bearer " + token
    }
  })
  .then(res => res.json())
  .then(books => {
    deleteBookListBody.innerHTML = ""; // очищуємо список
    books.forEach(book => {
      const row = document.createElement("tr");

      row.innerHTML = `
        <td>${book.id}</td>
        <td>${book.title}</td>
        <td>${book.author}</td>
        <td>${book.publicationYear || ''}</td>
        <td>${book.publisher || ''}</td>
        <td><button data-id="${book.id}" class="deleteBookBtn">Видалити</button></td>
      `;

      deleteBookListBody.appendChild(row);
    });

    // Прив'язати обробник події до кнопок видалення
    document.querySelectorAll(".deleteBookBtn").forEach(btn => {
      btn.addEventListener("click", (e) => {
        const bookId = e.target.getAttribute("data-id");
        if (confirm(`Ви впевнені, що хочете видалити книгу ID ${bookId}?`)) {
          deleteBook(bookId);
        }
      });
    });
  })
  .catch(err => {
    alert("Помилка завантаження книг: " + err.message);
  });
}

// Видалення книги
function deleteBook(id) {
  fetch(`http://localhost:5278/api/books/${id}`, {
    method: "DELETE",
    headers: {
      "Authorization": "Bearer " + token
    }
  })
  .then(res => {
    if (res.ok) {
      alert(`Книга ID ${id} успішно видалена!`);
      loadBooksForDelete();  // обновити список після видалення
      loadBooks(document.getElementById('searchInput').value);
    } else {
      return res.text().then(text => { throw new Error(text || "Помилка видалення книги"); });
    }
  })
  .catch(err => {
    alert("Сталася помилка: " + err.message);
  });
}



const btnAddGenre = document.getElementById("btnAddGenre");
const addGenreModal = document.getElementById("addGenreModal");
const closeAddGenreModal = document.getElementById("closeAddGenreModal");
const addGenreForm = document.getElementById("addGenreForm");

btnAddGenre.addEventListener("click", () => {
  addGenreModal.style.display = "flex";
  document.body.style.overflow = "hidden"; // Блокувати скрол основної сторінки
});

closeAddGenreModal.addEventListener("click", () => {
  addGenreModal.style.display = "none";
  document.body.style.overflow = ""; // Відновити скрол
});

addGenreForm.addEventListener("submit", (e) => {
  e.preventDefault();

  const genreName = document.getElementById("genreName").value.trim();
  if (!genreName) {
    alert("Введіть назву жанру");
    return;
  }

  const token = localStorage.getItem("token"); // отримуємо токен
fetch("http://localhost:5278/api/genres", {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
    "Authorization": "Bearer " + localStorage.getItem("token")
  },
  body: JSON.stringify({ id: 0, name: genreName })
})
.then(res => {
  if (!res.ok) {
    return res.text().then(text => { throw new Error(text || "Помилка сервера"); });
  }
  return res.text().then(text => text ? JSON.parse(text) : null);
})
.then(data => {
  alert("Жанр додано!");
  // тут оновлюємо список жанрів
  loadGenresFilter();
  addGenreModal.style.display = "none";
  document.body.style.overflow = "";
  addGenreForm.reset();
})
.catch(err => {
  alert("Сталася помилка: " + err.message);
});
});


  const editGenreBtn = document.getElementById("editGenreName");
  const editGenreModal = document.getElementById("editGenreModal");
  const closeEditGenreModalBtn = document.getElementById("closeEditGenreModal");
  const editGenreSelect = document.getElementById("editGenreSelect");
  const editGenreNameInput = document.getElementById("editGenreNameInput");
  const saveGenreChangesBtn = document.getElementById("saveGenreChanges");

  editGenreBtn.addEventListener("click", () => {
    loadGenresForEdit();
    editGenreModal.style.display = "flex";
    document.body.style.overflow = "hidden";
  });

  closeEditGenreModalBtn.addEventListener("click", () => {
    editGenreModal.style.display = "none";
    document.body.style.overflow = "auto";
  });

  // Завантаження жанрів
  function loadGenresForEdit() {
    fetch("http://localhost:5278/api/genres", {
      headers: { "Authorization": "Bearer " + token }
    })
    .then(res => {
      if (!res.ok) throw new Error("Не вдалося завантажити жанри");
      return res.json();
    })
    .then(genres => {
      editGenreSelect.innerHTML = '<option value="">-- Оберіть жанр --</option>';
      genres.forEach(g => {
        const opt = document.createElement("option");
        opt.value = g.id;
        opt.textContent = g.name;
        editGenreSelect.appendChild(opt);
      });
      editGenreNameInput.value = "";
    })
    .catch(err => alert(err.message));
  }

  // При зміні вибору жанру — підставляємо назву у поле
  editGenreSelect.addEventListener("change", () => {
    const selectedOption = editGenreSelect.options[editGenreSelect.selectedIndex];
    editGenreNameInput.value = selectedOption.value ? selectedOption.textContent : "";
  });

  // Відправка PUT-запиту
  saveGenreChangesBtn.addEventListener("click", () => {
    const id = editGenreSelect.value;
    const newName = editGenreNameInput.value.trim();

    if (!id) {
      return alert("Оберіть жанр для редагування");
    }
    if (!newName) {
      return alert("Введіть нову назву жанру");
    }

    const data = {
      id: parseInt(id),
      name: newName
    };

    fetch(`http://localhost:5278/api/genres/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": "Bearer " + token
      },
      body: JSON.stringify(data)
    })
    .then(res => {
      if (!res.ok) {
        return res.text().then(text => { throw new Error(text || "Помилка оновлення жанру"); });
      }
      return res.json().catch(() => null);
    })
    .then(() => {
      alert("Жанр успішно оновлено");
      loadGenresFilter();
      editGenreModal.style.display = "none";
      document.body.style.overflow = "auto";
      // Тут можна додати оновлення списку жанрів на сторінці, якщо потрібно
    })
    .catch(err => alert("Помилка: " + err.message));
  });



  const deleteGenreBtn = document.getElementById("deleteGenre");
const deleteGenreModal = document.getElementById("deleteGenreModal");
const closeDeleteGenreModal = document.getElementById("closeDeleteGenreModal");
const deleteGenreList = document.getElementById("deleteGenreList");


// Відкриваємо модалку та завантажуємо жанри
deleteGenreBtn.addEventListener("click", () => {
  loadGenresForDelete();
  deleteGenreModal.style.display = "flex";
  document.body.style.overflow = "hidden";
});

// Закриття модалки
closeDeleteGenreModal.addEventListener("click", () => {
  deleteGenreModal.style.display = "none";
  document.body.style.overflow = "auto";
});

// Функція завантаження жанрів у список для видалення
function loadGenresForDelete() {
  fetch("http://localhost:5278/api/genres", {
    headers: {
      "Authorization": "Bearer " + token
    }
  })
  .then(res => {
    if (!res.ok) throw new Error("Не вдалося завантажити жанри");
    return res.json();
  })
  .then(genres => {
    deleteGenreList.innerHTML = ""; // очищаємо список
    genres.forEach(genre => {
      const li = document.createElement("li");
      li.style.display = "flex";
      li.style.justifyContent = "space-between";
      li.style.alignItems = "center";
      li.style.marginBottom = "8px";

       const despan = document.createElement("span");
       despan.style.textAlign = "left";
       despan.style.width = "80%";
      despan.textContent = genre.name;

      const delBtn = document.createElement("button");
      delBtn.textContent = "Видалити";
      delBtn.style.marginLeft = "10px";
      delBtn.className = "btndelete";

      // Обробник видалення
      delBtn.addEventListener("click", () => {
        if (confirm(`Ви впевнені, що хочете видалити жанр "${genre.name}"?`)) {
          deleteGenre(genre.id);
        }
      });

      li.appendChild(despan);
      li.appendChild(delBtn);
      deleteGenreList.appendChild(li);
    });
  })
  .catch(err => {
    alert("Помилка завантаження жанрів: " + err.message);
  });
}

// Функція видалення жанру
function deleteGenre(id) {
  fetch(`http://localhost:5278/api/genres/${id}`, {
    method: "DELETE",
    headers: {
      "Authorization": "Bearer " + token
    }
  })
  .then(res => {
    if (!res.ok) {
      return res.text().then(text => { throw new Error(text || "Помилка при видаленні"); });
    }
    alert("Жанр успішно видалено");
    loadGenresForDelete();   // оновлюємо список жанрів після видалення
    loadGenresFilter();      // оновлення фільтра жанрів, якщо є така функція
  })
  .catch(err => {
    alert("Помилка: " + err.message);
  });
}


document.getElementById("btnReporting").addEventListener("click", () => {
  window.location.href = "reporting.html";
});



 document.getElementById('btnExit').addEventListener('click', () => {
    window.location.href = 'index.html'; // або '/' якщо це головна сторінка сайту
  });


});
