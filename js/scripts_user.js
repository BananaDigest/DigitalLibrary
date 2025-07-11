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
       // img.src = `image/books/${book.title}.jpg`;
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
let selectedBook = null;
//const userId = new URLSearchParams(window.location.search).get('id');

function showBookModal(book) {

    selectedBook = book;

  document.getElementById('modalImage').src = `file:///D:/Coding/DigitalLibrary/API/image/books/${book.title}.jpg`;
  document.getElementById('modalTitle').textContent = book.title;
  document.getElementById('modalAuthor').textContent = book.author;
  document.getElementById('modalPublisher').textContent = book.publisher;
  document.getElementById('modalYear').textContent = book.publicationYear;
  document.getElementById('modalGenre').textContent = book.genreName;
  document.getElementById('modalDescription').textContent = book.description || 'Опис відсутній';

  // Очистити раніше додані радіокнопки
  const bookTypeSelection = document.getElementById('bookTypeSelection');
  bookTypeSelection.innerHTML = '<strong>Тип книги:</strong>';

  document.getElementById('btnOrder').style.display = 'none';
  document.getElementById('btnDownload').style.display = 'none';
  document.getElementById('btnListen').style.display = 'none';

  fetch('http://localhost:5278/api/booktypes')
    .then(res => res.json())
    .then(types => {
      // Відфільтрувати типи по тих, які є в book.availableTypeIds
      const filteredTypes = types.filter(type => book.availableTypeIds.includes(type.id));

      filteredTypes.forEach(type => {
        const label = document.createElement('label');
        label.style.display = 'inline-block';
        label.style.margin = '0 10px';

        const radio = document.createElement('input');
        radio.type = 'radio';
        radio.style.width = 'auto';
        radio.name = 'modalBookType';
        radio.value = type.name; // або type.id, якщо зручніше
        label.appendChild(radio);

        label.appendChild(document.createTextNode(' ' + type.name));
        bookTypeSelection.appendChild(label);
      });


    
  // Скидаємо вибір радіокнопок (щоб ніщо не було вибрано)
  bookTypeSelection.querySelectorAll('input[name="modalBookType"]').forEach(radio => {
    radio.checked = false;
  });

  // Сховати всі кнопки і повідомлення
  document.getElementById('btnOrder').style.display = 'none';
  document.getElementById('btnDownload').style.display = 'none';
  document.getElementById('btnListen').style.display = 'none';
  document.getElementById('noStockMessage').style.display = 'none';

  // Обробник зміни вибору типу
  bookTypeSelection.querySelectorAll('input[name="modalBookType"]').forEach(radio => {
    radio.addEventListener('change', event => {
      const selectedType = event.target.value;

      document.getElementById('btnOrder').style.display = 'none';
      document.getElementById('btnDownload').style.display = 'none';
      document.getElementById('btnListen').style.display = 'none';
      document.getElementById('noStockMessage').style.display = 'none';

      if (selectedType === 'Paper') {
        if (book.availableCopies > 0) {
          document.getElementById('btnOrder').style.display = 'inline-block';
          document.getElementById('noStockMessage').style.display = 'none';
        } else {
          document.getElementById('btnOrder').style.display = 'none';
          document.getElementById('noStockMessage').style.display = 'block';
        }
      } else if (selectedType === 'Electronic') {
        document.getElementById('btnDownload').style.display = 'inline-block';
      } else if (selectedType === 'Audio') {
        document.getElementById('btnListen').style.display = 'inline-block';
      }
    });
  });
    })
    .catch(err => {
      console.error('Не вдалося завантажити типи книг:', err);
    });

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
const btnAllOrder = document.getElementById('btnAllOrder');
const ordersModal = document.getElementById('ordersModal');
const ordersModalClose = document.getElementById('ordersModalClose');
const userId = getUserIdFromURL();

function getUserIdFromURL() {
  const params = new URLSearchParams(window.location.search);
  return params.get('id');
}

btnAllOrder.addEventListener('click', () => {
  const authToken = localStorage.getItem('authToken');
  if (!authToken) {
    alert('Будь ласка, увійдіть в систему');
    return;
  }
  openOrdersModal();
});

ordersModalClose.addEventListener('click', () => {
  ordersModal.style.display = 'none';
});

// Закрити при кліку поза модалкою
ordersModal.addEventListener('click', (e) => {
  if (e.target === ordersModal) {
    ordersModal.style.display = 'none';
  }
});

function openOrdersModal() {
  const authToken = localStorage.getItem('authToken');
  const listContainer = document.getElementById('ordersList');
  listContainer.innerHTML = '<p>Завантаження...</p>';

  // Відкрити модальне вікно
  ordersModal.style.display = 'flex';

  fetch(`http://localhost:5278/api/orders/by-user/${userId}`, {
    headers: {
      'Authorization': 'Bearer ' + authToken,
      'Content-Type': 'application/json',
    }
  })
  .then(res => {
    if (!res.ok) throw new Error('Не вдалося отримати замовлення');
    return res.json();
  })
  .then(orders => {
    const filteredOrders = orders.filter(o => o.status === 1 && o.orderType === 1);
    if (filteredOrders.length === 0) {
      listContainer.innerHTML = '<p>У вас немає паперових замовлень</p>';
      return;
    }
    return fetch('http://localhost:5278/api/books', {
      headers: {
        'Authorization': 'Bearer ' + authToken
      }
    })
    .then(res => {
      if (!res.ok) throw new Error('Не вдалося отримати книги');
      return res.json();
    })
    .then(books => {
      listContainer.innerHTML = '';
      filteredOrders.forEach(order => {
        const book = books.find(b => b.id === order.bookId);
        if (!book) return;

        const bookItem = document.createElement('div');
        bookItem.style.display = 'flex';
        bookItem.style.justifyContent = 'space-between';
        bookItem.style.alignItems = 'center';
        bookItem.style.marginBottom = '8px';
        bookItem.style.borderBottom = '1px solid #ccc';
        bookItem.style.paddingBottom = '5px';

        const bookInfo = document.createElement('div');
        bookInfo.style.width = '99%';
        bookInfo.style.textAlign = 'left'
        bookInfo.innerText = `${book.title} — ${book.author} (${book.publicationYear})`;

        const deleteBtn = document.createElement('button');
        deleteBtn.innerText = '✖';
        deleteBtn.style.cursor = 'pointer';
        deleteBtn.style.border = 'none';
        deleteBtn.style.background = 'none';
        deleteBtn.style.color = 'red';
        deleteBtn.style.fontSize = '18px';

        deleteBtn.addEventListener('click', () => {
          if (confirm(`Видалити замовлення книги "${book.title}"?`)) {
            deleteOrder(order.id, bookItem);
          }
        });

        bookItem.appendChild(bookInfo);
        bookItem.appendChild(deleteBtn);
        listContainer.appendChild(bookItem);
      });
    });
  })
  .catch(err => {
    listContainer.innerHTML = `<p style="color:red;">${err.message}</p>`;
  });
}

function deleteOrder(orderId, domElement) {
  const authToken = localStorage.getItem('authToken');
  const listContainer = document.getElementById('ordersList');

  fetch(`http://localhost:5278/api/orders/${orderId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': 'Bearer ' + authToken,
      'Content-Type': 'application/json'
    }
  })
  .then(async res => {
    if (!res.ok) {
      let errorMessage = 'Не вдалося видалити замовлення';
      try {
        const errorData = await res.json();
        if (errorData.message) errorMessage = errorData.message;
        else if (typeof errorData === 'string') errorMessage = errorData;
      } catch(e) {}
      throw new Error(errorMessage + ` (код статусу: ${res.status})`);
    }

    domElement.remove();

    // Перевіряємо, чи лишились ще елементи
    const remainingOrders = listContainer.querySelectorAll('div');
    if (remainingOrders.length === 0) {
      listContainer.innerHTML = '<p>У вас немає паперових замовлень</p>';
    }
  })
  .catch(err => {
    alert('Помилка видалення замовлення: ' + err.message);
    console.error('Delete order error:', err);
  });
}

// Допоміжна функція для отримання userId з URL
function getUserIdFromURL() {
  const urlParams = new URLSearchParams(window.location.search);
  return parseInt(urlParams.get('id'));
}

// Основна функція замовлення
function sendOrder(bookType) {
  const authToken = localStorage.getItem('authToken');
  console.log('Token:', authToken);

  if (!authToken) {
    alert('Будь ласка, увійдіть в систему');
    return;
  }

  const userId = getUserIdFromURL();
  const bookId = selectedBook?.id;

  console.log('User ID:', userId);
  console.log('Book ID:', bookId);
  console.log('Book Type:', bookType);

  if (!bookId || !userId) {
    alert('Дані для замовлення некоректні.');
    return;
  }

  fetch('http://localhost:5278/api/orders', {
    method: 'POST',
    mode: 'cors', // на випадок CORS
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authToken}`
    },
    body: JSON.stringify({
      userId: userId,
      bookId: bookId,
      orderType: bookType
    })
  })
  .then(async res => {
    const responseText = await res.text();

    console.log('Response Status:', res.status);
    console.log('Response Body:', responseText);

    if (!res.ok) {
      throw new Error(responseText || `HTTP error ${res.status}`);
    }

    alert('Замовлення оформлено успішно!');
    if (typeof bookModal !== 'undefined') {
      bookModal.style.display = 'none';
    }
  })
  .catch(err => {
    console.error('Помилка при оформленні замовлення:', err);
    alert(`Не вдалося оформити замовлення. Спробуйте пізніше.\n\n${err.message}`);
  });
}

// Прив’язка до кнопок
document.getElementById('btnOrder').addEventListener('click', () => sendOrder(1));    // Паперова книга
document.getElementById('btnDownload').addEventListener('click', () => sendOrder(2)); // Електронна
document.getElementById('btnListen').addEventListener('click', () => sendOrder(3));   // Аудіо



 document.getElementById('btnExit').addEventListener('click', () => {
    window.location.href = 'index.html'; // або '/' якщо це головна сторінка сайту
  });



});
