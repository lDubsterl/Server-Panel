import React, { useState, useEffect } from 'react';
import { Routes, Route, useNavigate } from 'react-router-dom';
import MinecraftServer from './Pages/MinecraftServer';
import Navbar from './components/Navbar';
import api from './services/api';
import './App.css';

const App = () => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    api.get('/Authentication/Verify')
  .then(response => {
    setIsAuthenticated(true);
    setIsAdmin(response.data);
  })
  .catch(() => setIsAuthenticated(false));
  }, []);

  const handleLogout = () => {
    api.post('/Authentication/LogOut')
      .then(() => {
        localStorage.removeItem('userId');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        setIsAuthenticated(false);
        navigate(0);
      })
      .catch();
  };

  const handleLogin = (isAdmin) => {

    setIsAuthenticated(true);
    setIsAdmin(isAdmin);
  };

  if (isAuthenticated == null)
    return;

  return (
    <div className="App">
       <Navbar isAuthenticated={isAuthenticated} onLogout={handleLogout} />
      <Routes>
        <Route path = "/" element = {<MinecraftServer/>}/>
      </Routes>
    </div>
  );
}

export default App;
