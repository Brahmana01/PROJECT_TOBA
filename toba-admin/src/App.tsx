import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Sidebar from './components/Sidebar';
import Dashboard from './pages/Dashboard';
import PlayersPage from './pages/PlayersPage';
import LocationsPage from './pages/LocationsPage';
import TriviaPage from './pages/TriviaPage';
import BattlePage from './pages/BattlePage';
import AchievementsPage from './pages/AchievementsPage';
import SettingsPage from './pages/SettingsPage';
import MusicPage from './pages/MusicPage';
import ChallengePage from './pages/ChallengePage';
import AdminLoginPage from './pages/AdminLoginPage';
import './styles/App.css';
import leafFooter from './assets/leaf-footer.png';

const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const token = localStorage.getItem('adminToken');

  return token ? <>{children}</> : <Navigate to="/login" replace />;
};

const AdminLayout = () => (
  <div className="admin-layout">
    <Sidebar />

    <div className="main-viewport">
      <Routes>
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/players" element={<PlayersPage />} />
        <Route path="/locations" element={<LocationsPage />} />
        <Route path="/trivia" element={<TriviaPage />} />
        <Route path="/battle" element={<BattlePage />} />
        <Route path="/achievements" element={<AchievementsPage />} />
        <Route path="/settings" element={<SettingsPage />} />
        <Route path="/music" element={<MusicPage />} />
        <Route path="/challenges" element={<ChallengePage />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>

      <footer className="footer-plank-container">
        <div className="wooden-plank-box">
          <img
            src={leafFooter}
            className="plank-leaf left"
            alt=""
          />

          <div className="plank-text-content">
            JELAJAH BUDAYA <span>•</span> TRIVIA SERU <span>•</span> BELAJAR MENYENANGKAN
          </div>

          <img
            src={leafFooter}
            className="plank-leaf right"
            alt=""
          />
        </div>
      </footer>
    </div>
  </div>
);

const App: React.FC = () => (
  <BrowserRouter>
    <Routes>
      <Route path="/login" element={<AdminLoginPage />} />

      <Route
        path="/*"
        element={
          <ProtectedRoute>
            <AdminLayout />
          </ProtectedRoute>
        }
      />
    </Routes>
  </BrowserRouter>
);

export default App;