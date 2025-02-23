import React, { useState, useEffect } from 'react';
import { Routes, Route, useNavigate } from 'react-router-dom';
import MinecraftServer from './Pages/MinecraftServer';
import './App.css';

const App = () => {
  return (
    <div className="App">
      <Routes>
        <Route path = "/" element = {<MinecraftServer/>}/>
      </Routes>
    </div>
  );
}

export default App;
