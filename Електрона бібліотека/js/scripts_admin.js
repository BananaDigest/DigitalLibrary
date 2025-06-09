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
          //img.src = `D:/Coding/DigitalLibrary/API/image/books${book.title}.jpg`;
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
  document.getElementById('modalImage').src = `image/books/${book.title}.jpg`;
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


document.getElementById('btnAllIssue').addEventListener('click', openIssueModal);



function openIssueModal() {
  const authToken = localStorage.getItem('authToken');
  const listContainer = document.getElementById('issueList');
  listContainer.innerHTML = '<p>Завантаження...</p>';

  document.getElementById('issueModal').style.display = 'flex';

  fetch('http://localhost:5278/api/orders', {
    headers: {
      'Authorization': 'Bearer ' + authToken
    }
  })
  .then(res => {
    if (!res.ok) throw new Error('Не вдалося отримати замовлення');
    return res.json();
  })
  .then(orders => {
    const filteredOrders = orders.filter(o => o.status === 1 && o.orderType === 1);
    if (filteredOrders.length === 0) {
      listContainer.innerHTML = '<p>Немає замовлень на видачу</p>';
      return;
    }

    return Promise.all([
      fetch('http://localhost:5278/api/auth/get-all-users', {
        headers: { 'Authorization': 'Bearer ' + authToken }
      }).then(r => r.json()),
      fetch('http://localhost:5278/api/books', {
        headers: { 'Authorization': 'Bearer ' + authToken }
      }).then(r => r.json())
    ]).then(([users, books]) => {
      listContainer.innerHTML = '';

      filteredOrders.forEach(order => {
        const user = users.find(u => u.id === order.userId);
        const book = books.find(b => b.id === order.bookId);
        if (!user || !book) return;

        const item = document.createElement('div');
        item.style.borderBottom = '1px solid #ccc';
        item.style.padding = '10px 0';
        item.style.display = 'flex';
        item.style.justifyContent = 'space-between';
        item.style.alignItems = 'center';

        const info = document.createElement('div');
        info.innerHTML = `
         <div><strong>Користувач:</strong> ${user.firstName} ${user.lastName} (${user.email})</div>
          <div><strong>Книга:</strong> ${book.title} (${book.publicationYear})</div>
          <div><strong>Автор:</strong> ${book.author}</div>
         
        `;

        const issueBtn = document.createElement('button');
        issueBtn.innerText = 'Видати';
        issueBtn.style.padding = '4px 10px';
        issueBtn.style.border = 'none';
        issueBtn.style.backgroundColor = '#28a745';
        issueBtn.style.color = 'white';
        issueBtn.style.cursor = 'pointer';
        issueBtn.style.borderRadius = '4px';

        issueBtn.addEventListener('click', () => {
          fetch(`http://localhost:5278/api/orders/${order.id}/status`, {
            method: 'PATCH',
            headers: {
              'Authorization': 'Bearer ' + authToken,
              'Content-Type': 'application/json'
            }
          })
          .then(res => {
            if (!res.ok) throw new Error('Не вдалося оновити статус');
            item.remove(); // При успіху видаляємо елемент з DOM
          })
          .catch(err => alert(err.message));
        });

        item.appendChild(info);
        item.appendChild(issueBtn);
        listContainer.appendChild(item);
      });
    });
  })
  .catch(err => {
    listContainer.innerHTML = `<p style="color:red;">${err.message}</p>`;
  });
}
const btnAcceptance = document.getElementById('btnAcceptance');
const acceptanceModal = document.getElementById('acceptanceModal');
const closeAcceptanceModal = document.getElementById('closeAcceptanceModal');
const acceptanceResults = document.getElementById('acceptanceResults');
const bookSearchInput = document.getElementById('bookSearchInput');

btnAcceptance.addEventListener('click', () => {
  acceptanceModal.style.display = 'flex';
  bookSearchInput.value = '';
  acceptanceResults.innerHTML = '<p>Введіть дані для пошуку...</p>';
});

closeAcceptanceModal.addEventListener('click', () => {
  acceptanceModal.style.display = 'none';
});

acceptanceModal.addEventListener('click', (e) => {
  if (e.target === acceptanceModal) {
    acceptanceModal.style.display = 'none';
  }
});

bookSearchInput.addEventListener('input', () => {
  const authToken = localStorage.getItem('authToken');
  const query = bookSearchInput.value.toLowerCase().trim();

  if (!authToken) {
    acceptanceResults.innerHTML = '<p style="color:red;">Будь ласка, увійдіть у систему</p>';
    return;
  }

  if (!query) {
    acceptanceResults.innerHTML = '<p>Введіть дані для пошуку...</p>';
    return;
  }

  acceptanceResults.innerHTML = '<p>Завантаження...</p>';

  // Отримуємо замовлення та книги паралельно
  Promise.all([
    fetch('http://localhost:5278/api/orders', {
      headers: { 'Authorization': 'Bearer ' + authToken }
    }),
    fetch('http://localhost:5278/api/books', {
      headers: { 'Authorization': 'Bearer ' + authToken }
    })
  ])
  .then(responses => {
    const [ordersRes, booksRes] = responses;
    if (!ordersRes.ok) throw new Error(`Помилка при завантаженні замовлень (код ${ordersRes.status})`);
    if (!booksRes.ok) throw new Error(`Помилка при завантаженні книг (код ${booksRes.status})`);
    return Promise.all([ordersRes.json(), booksRes.json()]);
  })
  .then(([orders, books]) => {
    const filteredOrders = orders.filter(o => o.status === 2 && o.orderType === 1);
    const matches = filteredOrders.map(order => {
      const book = books.find(b => b.id === order.bookId);
      if (!book) return null;
      const text = `${order.id} ${book.title} ${book.author} ${book.publicationYear} ${book.publisher}`.toLowerCase();
      if (text.includes(query)) {
        return { orderId: order.id, ...book };
      }
      return null;
    }).filter(Boolean);

    if (matches.length === 0) {
      acceptanceResults.innerHTML = '<p>Нічого не знайдено</p>';
      return;
    }

    acceptanceResults.innerHTML = '';

    matches.forEach(match => {
      const item = document.createElement('div');
      item.style.display = 'flex';
      item.style.justifyContent = 'flex-start';
      item.style.alignItems = 'center';
      item.style.marginBottom = '10px';
      item.dataset.orderId = match.orderId;

      const numorder = document.createElement('span');
      numorder.style.paddingRight = '10px';
      numorder.textContent = `№${match.orderId}:`;

      const info = document.createElement('div');
      info.style.textAlign = 'left';
      info.style.width = '75%';
      info.textContent = `${match.title} — ${match.author} (${match.publicationYear}) [${match.publisher}]`;

      const acceptBtn = document.createElement('button');
      acceptBtn.textContent = 'Прийняти';
      acceptBtn.style.backgroundColor = '#4CAF50';
      acceptBtn.style.color = 'white';
      acceptBtn.style.border = 'none';
      acceptBtn.style.padding = '6px 12px';
      acceptBtn.style.cursor = 'pointer';

      acceptBtn.addEventListener('click', () => {
        if (confirm(`Прийняти книгу "${match.title}" назад у бібліотеку?`)) {
          deleteOrder(match.orderId, item);
        }
      });

      item.appendChild(numorder);
      item.appendChild(info);
      item.appendChild(acceptBtn);
      acceptanceResults.appendChild(item);
    });
  })
  .catch(err => {
    acceptanceResults.innerHTML = `<p style="color:red;">${err.message}</p>`;
    console.error(err);
  });
});

function deleteOrder(orderId, domElement) {
  const authToken = localStorage.getItem('authToken');
  const listContainer = document.getElementById('issueList');

  console.log('Deleting order ID:', orderId);

  fetch(`http://localhost:5278/api/orders/${orderId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': 'Bearer ' + authToken,
      'Content-Type': 'application/json'
    }
  })
  .then(res => {
    return res.text().then(text => {
      console.log('DELETE response status:', res.status);
      console.log('DELETE response body:', text);

      if (!res.ok) {
        let errorMessage = 'Не вдалося видалити замовлення';
        try {
          const errorData = JSON.parse(text);
          if (errorData.message) errorMessage = errorData.message;
          else if (typeof errorData === 'string') errorMessage = errorData;
        } catch(e) {
          errorMessage = text || errorMessage;
        }
        throw new Error(errorMessage + ` (код статусу: ${res.status})`);
      }

      return text;
    });
  })
  .then(() => {
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



const btnAllUsers = document.getElementById('btnAllUsers');
const usersModal = document.getElementById('usersModal');
const closeUsersModal = document.getElementById('closeUsersModal');
const usersResults = document.getElementById('usersResults');
const userSearchInput = document.getElementById('userSearchInput');

btnAllUsers.addEventListener('click', () => {
  usersModal.style.display = 'flex';
  userSearchInput.value = '';
  usersResults.innerHTML = '<p>Введіть дані для пошуку...</p>';
});

closeUsersModal.addEventListener('click', () => {
  usersModal.style.display = 'none';
});

usersModal.addEventListener('click', (e) => {
  if (e.target === usersModal) {
    usersModal.style.display = 'none';
  }
});

userSearchInput.addEventListener('input', () => {
  const authToken = localStorage.getItem('authToken');
  const query = userSearchInput.value.toLowerCase().trim();

  if (!authToken) {
    usersResults.innerHTML = '<p style="color:red;">Будь ласка, увійдіть у систему</p>';
    return;
  }

  if (!query) {
    usersResults.innerHTML = '<p>Введіть дані для пошуку...</p>';
    return;
  }

  usersResults.innerHTML = '<p>Завантаження...</p>';

  fetch('http://localhost:5278/api/auth/get-all-users', {
    headers: { 'Authorization': 'Bearer ' + authToken }
  })
  .then(res => {
    if (!res.ok) throw new Error(`Помилка при завантаженні користувачів (код ${res.status})`);
    return res.json();
  })
  .then(users => {
    // Фільтруємо за прізвищем, ім’ям, роком народження, персональним id
    const matches = users.filter(user => {
      const text = `${user.lastName} ${user.firstName} ${user.id}`.toLowerCase();
      return text.includes(query);
    });

    if (matches.length === 0) {
      usersResults.innerHTML = '<p>Нічого не знайдено</p>';
      return;
    }

    usersResults.innerHTML = '';

    matches.forEach(user => {
      const item = document.createElement('div');
      item.style.display = 'flex';
      item.style.justifyContent = 'space-between';
      item.style.alignItems = 'center';
      item.style.marginBottom = '10px';
      item.dataset.userId = user.id;

      const info = document.createElement('div');
      info.style.textAlign = 'left';
      info.style.width = '80%';
      info.textContent = `${user.lastName} ${user.firstName} [ID: ${user.id}]`;

      const deleteBtn = document.createElement('button');
      deleteBtn.textContent = 'Видалити';
      deleteBtn.style.backgroundColor = '#d9534f';
      deleteBtn.style.color = 'white';
      deleteBtn.style.border = 'none';
      deleteBtn.style.padding = '6px 12px';
      deleteBtn.style.cursor = 'pointer';

      deleteBtn.addEventListener('click', () => {
        if (confirm(`Видалити користувача "${user.lastName} ${user.firstName}"?`)) {
          deleteUser(user.id, item);
        }
      });

      item.appendChild(info);
      item.appendChild(deleteBtn);
      usersResults.appendChild(item);
    });
  })
  .catch(err => {
    usersResults.innerHTML = `<p style="color:red;">${err.message}</p>`;
    console.error(err);
  });
});

function deleteUser(userId, domElement) {
  const authToken = localStorage.getItem('authToken');

  fetch(`http://localhost:5278/api/auth/${userId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': 'Bearer ' + authToken,
      'Content-Type': 'application/json'
    }
  })
  .then(res => {
    return res.text().then(text => {
      if (!res.ok) {
        let errorMessage = 'Не вдалося видалити користувача';
        try {
          const errorData = JSON.parse(text);
          if (errorData.message) errorMessage = errorData.message;
          else if (typeof errorData === 'string') errorMessage = errorData;
        } catch(e) {
          errorMessage = text || errorMessage;
        }
        throw new Error(errorMessage + ` (код статусу: ${res.status})`);
      }
      return text;
    });
  })
  .then(() => {
    domElement.remove();

    // Якщо немає більше користувачів
    if (usersResults.children.length === 0) {
      usersResults.innerHTML = '<p>Користувачі відсутні</p>';
    }
  })
  .catch(err => {
    alert('Помилка видалення користувача: ' + err.message);
    console.error('Delete user error:', err);
  });
}



 document.getElementById('btnExit').addEventListener('click', () => {
    window.location.href = 'index.html'; // або '/' якщо це головна сторінка сайту
  });


});
