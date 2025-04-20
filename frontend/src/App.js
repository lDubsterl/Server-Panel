import React, { useState, useEffect } from 'react';
import { Routes, Route, useNavigate, Switch } from 'react-router-dom';
import Server from './Pages/Server';
import Navbar from './components/Navbar';
import ApiConfig from './services/api';
import './App.css';
import ServerSelection from './Pages/ServerSelection';

const App = () => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    ApiConfig.api.get('/Authentication/Verify')
      .then(response => {
        setIsAuthenticated(true);
        setIsAdmin(response.data);
      })
      .catch(() => setIsAuthenticated(false));
  }, []);

  const handleLogout = () => {
    ApiConfig.api.post('/Authentication/LogOut')
      .then(() => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        setIsAuthenticated(false);
        navigate(0);
      })
      .catch();
  };

  if (isAuthenticated == null)
    return;

  let id = localStorage.getItem("id");
  return (
    <div className="App">
      <Navbar isAuthenticated={isAuthenticated} onLogout={handleLogout} />
      <Routes>
        <Route path=":id" element={<ServerSelection />} />
        <Route path=':id/:serverTypeName' element={<Server />} />
      </Routes>
    </div>
  );
}

export default App;
