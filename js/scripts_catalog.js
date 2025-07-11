document.addEventListener("DOMContentLoaded", function () {
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
fetch('http://localhost:5278/api/genres')
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
  });

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
  document.getElementById('modalImage').src = `file:///D:/Coding/DigitalLibrary/API/image/books/${book.title}.jpg`;
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


});
