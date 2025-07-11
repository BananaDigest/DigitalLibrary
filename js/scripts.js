document.addEventListener("DOMContentLoaded", function () {
  const modal = document.getElementById('authModal');
  const login = document.getElementById('loginForm');
  const register = document.getElementById('registerForm');

  // Відкрити модальне вікно з потрібною формою
  function openModal(mode) {
    modal.style.display = 'flex';
    switchForm(mode);
  }

  // Закрити модальне вікно
  function closeModal() {
    modal.style.display = 'none';
  }

  // Переключення між формами входу і реєстрації
  function switchForm(mode) {
    if (mode === 'login') {
      login.style.display = 'block';
      register.style.display = 'none';
    } else {
      login.style.display = 'none';
      register.style.display = 'block';
    }
  }

  // Закриття модального при кліку поза ним
  window.onclick = function (e) {
    if (e.target === modal) {
      closeModal();
    }
  };

  // Обробники кнопок відкриття модального вікна
  const btnLogin = document.getElementById("btnLogin");
  const btnRegister = document.getElementById("btnRegister");
  const btnClose = document.getElementById("modalClose");

  if (btnLogin) btnLogin.onclick = () => openModal("login");
  if (btnRegister) btnRegister.onclick = () => openModal("register");
  if (btnClose) btnClose.onclick = closeModal;

  // Обробники посилань-перемикачів між формами
  const switchToRegister = document.getElementById("switchToRegister");
  const switchToLogin = document.getElementById("switchToLogin");

  if (switchToRegister) {
    switchToRegister.onclick = (e) => {
      e.preventDefault();
      switchForm('register');
    };
  }

  if (switchToLogin) {
    switchToLogin.onclick = (e) => {
      e.preventDefault();
      switchForm('login');
    };
  }

  // Перехід до каталогу
  const catalogBtn = document.querySelector(".btn-cat button");
  if (catalogBtn) {
    catalogBtn.onclick = () => window.location.href = `catalog.html`;
  }
const loginBtn = document.getElementById('loginBtn');
let authToken = '';

if (loginBtn) {
  loginBtn.addEventListener('click', () => {
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    fetch('http://localhost:5278/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: email, password: password }),
    })
    .then(res => {
      if (!res.ok) throw new Error('Невірний логін або пароль');
      return res.json();
    })
    .then(data => {
      authToken = data.token; // зберігаємо токен
      localStorage.setItem('authToken', data.token); // Зберігаємо токен у localStorage
      return fetch('http://localhost:5278/api/auth/get-all-users', {
        headers: {
          'Authorization': 'Bearer ' + authToken
        }
      });
    })
    .then(res => {
      if (!res.ok) throw new Error('Не вдалося отримати користувачів');
      return res.json();
    })
    .then(users => {
      const user = users.find(u => u.email === email);
      if (!user) throw new Error('Користувач не знайдений');

      // Перевіряємо роль користувача і переходимо на відповідну сторінку
      switch (user.role) {
        case 'Administrator':
          window.location.href = `admin.html?id=${user.id}`;
          break;
        case 'Manager':
          window.location.href = `manager.html?id=${user.id}`;
          break;
        default:
          window.location.href = `user.html?id=${user.id}`;
          break;
      }
    })
    .catch(err => {
      alert(err.message);
      console.error(err);
    });
  });
}

const registerBtn = document.getElementById('registerBtn');

if (registerBtn) {
  registerBtn.addEventListener('click', () => {
    const firstName = document.getElementById('registerFirstName').value;
    const lastName = document.getElementById('registerLastName').value;
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;

    // 1. Запит на реєстрацію
    fetch('http://localhost:5278/api/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ firstName, lastName, email, password })
    })
    .then(res => {
      if (!res.ok) throw new Error('Помилка під час реєстрації');
      return res.text(); // Сервер може не повертати JSON
    })
    .then(text => {
      if (text) {
        try {
          const data = JSON.parse(text);
          console.log('Реєстрація:', data.message);
        } catch (e) {
          console.warn('Сервер не повернув JSON');
        }
      }

      // 2. Вхід одразу після успішної реєстрації
      return fetch('http://localhost:5278/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: email, password: password })
      });
    })
    .then(res => {
      if (!res.ok) throw new Error('Не вдалося увійти після реєстрації');
      return res.json();
    })
    .then(data => {
      const authToken = data.token;

      // 3. Отримання всіх користувачів
      return fetch('http://localhost:5278/api/auth/get-all-users', {
        headers: {
          'Authorization': 'Bearer ' + authToken
        }
      }).then(res => {
        if (!res.ok) throw new Error('Не вдалося отримати користувачів');
        return res.json().then(users => ({ users, authToken }));
      });
    })
    .then(({ users, authToken }) => {
      const email = document.getElementById('registerEmail').value;
      const user = users.find(u => u.email === email);

      if (!user) throw new Error('Користувач не знайдений');

      // 4. Перенаправлення за роллю
      switch (user.role) {
        case 'Administrator':
          window.location.href = `admin.html?id=${user.id}`;
          break;
        case 'Manager':
          window.location.href = `manager.html?id=${user.id}`;
          break;
        default:
          window.location.href = `user.html?id=${user.id}`;
      }
    })
    .catch(err => {
      alert(err.message);
      console.error(err);
    });
  });
}




});
