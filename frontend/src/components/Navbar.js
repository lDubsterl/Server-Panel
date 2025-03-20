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
        <li><Link to="/Servers">Панель управления</Link></li>
        {isAuthenticated ? (
          <li className={styles.dropdown}>
            <img
              src="https://digital.spmi.ru/article/images/blank.png"
              alt="User Avatar"
              className={styles.avatar}
              onClick={toggleDropdown}
            />
          </li>) : (
          <li>
            <div className={styles["authentication-actions-container"]}>
              <Link to="/Login">Вход</Link>
              <label style={{padding: '0px 5px', margin: '0'}}>/</label>
              <Link to="/Register">Регистрация</Link>
            </div>
          </li>
        )}
        {dropdownOpen && (
          <ul className={styles['dropdown-menu']}>
            {isAuthenticated ? (
              <>
                <li><Link to={`/profile`}>Profile</Link></li>
                <li><a onClick={onLogout}>Logout</a></li>
              </>
            ) : (
              <>
                <li><Link to="/login">Login</Link></li>
                <li><Link to="/register">Register</Link></li>
              </>
            )}
          </ul>
        )}
      </ul>
    </nav >
  );
};

export default Navbar;