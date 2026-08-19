import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  LayoutDashboard, MapPin, HelpCircle, Users,
  Trophy, Settings, Sword, LogOut, ChevronRight,
  ChevronLeft, Menu, Music
} from 'lucide-react';
import '../styles/Sidebar.css';
import logoToba     from '../assets/logo-toba.png';
import houseSidebar from '../assets/house-sidebar.png';

const Sidebar: React.FC = () => {
  const navigate    = useNavigate();
  const location    = useLocation();
  const [collapsed, setCollapsed] = useState(false);

  const menu = [
    { name: 'Dashboard',   icon: <LayoutDashboard size={18}/>, path: '/dashboard'    },
    { name: 'Pemain',      icon: <Users size={18}/>,           path: '/players'      },
    { name: 'Lokasi',      icon: <MapPin size={18}/>,          path: '/locations'    },
    { name: 'Trivia',      icon: <HelpCircle size={18}/>,      path: '/trivia'       },
    { name: 'Battle',      icon: <Sword size={18}/>,           path: '/battle'       },
    { name: 'Achievement', icon: <Trophy size={18}/>,          path: '/achievements' },
    { name: 'Pengaturan',  icon: <Settings size={18}/>,        path: '/settings'     },
    { name: 'Musik & Audio', icon: <Music size={18}/>, path: '/music' },
  ];

  return (
    <aside className={`toba-sidebar ${collapsed ? 'collapsed' : ''}`}>

      {/* ── TOMBOL TOGGLE COLLAPSE ── */}
      <button
        className="sidebar-toggle-btn"
        onClick={() => setCollapsed(c => !c)}
        title={collapsed ? 'Buka Sidebar' : 'Tutup Sidebar'}
      >
        {collapsed ? <Menu size={18}/> : <ChevronLeft size={18}/>}
      </button>

      {/* ── AREA SCROLL (logo + nav + bottom) ── */}
      <div className="sidebar-scroll-area">

        {/* LOGO */}
        <div className="sidebar-logo">
          <img
            src={logoToba}
            alt="Logo TOBA"
            className="sidebar-main-img"
          />
          {!collapsed && (
            <>
              <h1>TOBA</h1>
              <p>Game Edukasi Budaya Batak Toba</p>
              <div className="admin-tag-luxury">ADMIN PANEL</div>
            </>
          )}
        </div>

        {/* NAV */}
        <nav className="sidebar-nav">
          {menu.map((m, i) => {
            const isActive = location.pathname === m.path;
            return (
              <div
                key={i}
                className={`nav-item ${isActive ? 'active' : ''}`}
                onClick={() => navigate(m.path)}
                title={collapsed ? m.name : ''}
              >
                <span className="icon">{m.icon}</span>
                {!collapsed && <span className="label">{m.name}</span>}
                {!collapsed && (
                  <ChevronRight
                    size={14}
                    className="arrow"
                    style={{ opacity: isActive ? 0.9 : 0.4 }}
                  />
                )}
              </div>
            );
          })}
        </nav>

        {/* BOTTOM */}
        <div className="sidebar-bottom">
          <div
            className="logout-btn"
            onClick={() => alert('Logout')}
            title={collapsed ? 'Keluar' : ''}
          >
            <LogOut size={18}/>
            {!collapsed && <span>Keluar</span>}
          </div>

          {!collapsed && (
            <>
              <img
                src={houseSidebar}
                alt="Rumah Bolon"
                style={{
                  width:'100%', borderRadius:'12px',
                  marginTop:'15px', border:'1px solid #5C3B26',
                  filter: 'drop-shadow(0 4px 12px rgba(0,0,0,0.6))'
                }}
              />
              <small style={{display:'block', marginTop:'10px', opacity:0.5, fontSize:'0.6rem'}}>
                © 2025 TOBA Game Admin v1.0.0
              </small>
            </>
          )}
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;