import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import styles from '../styles/Navbar.module.css';

const Navbar = ({ isAuthenticated, onLogout }) => {
  const [dropdownOpen, setDropdownOpen] = useState(false);

  const toggleDropdown = () => {
    setDropdownOpen(!dropdownOpen);
  };

  return (
    <nav className={styles.navbar}>
      <ul>
        <li><Link to="/">Главная</Link></li>
        <li><Link to={`/${localStorage.getItem('id')}`}>Панель управления</Link></li>
        {isAuthenticated ? (
          <li className={styles.dropdown}>
            <label onClick={toggleDropdown}> Настройки</label>
          </li>) : (
          <li>
            <div className={styles["authentication-actions-container"]}>
              <Link to="/login">Вход</Link>
              <label style={{ padding: '0px 5px', margin: '0' }}>/</label>
              <Link to="/register">Регистрация</Link>
            </div>
          </li>
        )}
      </ul>
      {dropdownOpen && isAuthenticated && (
          <ul className={styles['dropdown-menu']}>
            {isAuthenticated &&
              <div onClick={() => setDropdownOpen(false)}>
                <li><Link to={`/${localStorage.getItem('id')}/profile`}>Профиль</Link></li>
                <li><a onClick={onLogout}>Выход</a></li>
              </div>}
          </ul>
        )}
    </nav >
  );
};

export default Navbar;