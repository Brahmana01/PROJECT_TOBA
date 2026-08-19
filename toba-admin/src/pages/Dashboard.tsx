import React, { useEffect, useState, useRef } from 'react';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, PieChart, Pie, Cell, Legend,
  BarChart, Bar
} from 'recharts';
import {
  Users, Map, HelpCircle, Star, Sword,
  Settings, BarChart as BarChartIcon, Bell,
  BarChart2, PieChart as PieIcon, BarChart3
} from 'lucide-react';
import { getAdminStats, getAllPlayers } from '../services/api';
import type { AdminStats, Player } from '../types';
import { useNavigate } from 'react-router-dom';
import '../styles/Dashboard.css';

import bannerToba from '../assets/banner-toba.jpg';
import logoToba from '../assets/logo-toba.png';

// ── DATA CHART ────────────────────────────────────────────
const dataChart = [
  { name: '30 Apr', val: 200 },
  { name: '1 Mei',  val: 600 },
  { name: '2 Mei',  val: 750 },
  { name: '3 Mei',  val: 1200 },
  { name: '4 Mei',  val: 2125 },
  { name: '5 Mei',  val: 1500 },
  { name: '6 Mei',  val: 1900 },
];

const PIE_COLORS = ['#82ca9d','#D4AF37','#2196F3','#9C27B0','#FF9800','F44336','#4CAF50'];

// ── HOOK: Animated Counter ────────────────────────────────
function useCountUp(target: number, duration = 1200, start = false) {
  const [count, setCount] = useState(0);
  useEffect(() => {
    if (!start || target === 0) return;
    let startTime: number | null = null;
    const step = (timestamp: number) => {
      if (!startTime) startTime = timestamp;
      const progress = Math.min((timestamp - startTime) / duration, 1);
      // Ease out cubic
      const eased = 1 - Math.pow(1 - progress, 3);
      setCount(Math.floor(eased * target));
      if (progress < 1) requestAnimationFrame(step);
    };
    requestAnimationFrame(step);
  }, [target, start, duration]);
  return count;
}

// ── HOOK: Intersection Observer (Fade Up) ─────────────────
function useFadeUp(delay = 0) {
  const ref = useRef<HTMLDivElement>(null);
  const [visible, setVisible] = useState(false);
  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => { if (entry.isIntersecting) setVisible(true); },
      { threshold: 0.1 }
    );
    if (ref.current) observer.observe(ref.current);
    return () => observer.disconnect();
  }, []);
  return {
    ref,
    style: {
      opacity:    visible ? 1 : 0,
      transform:  visible ? 'translateY(0)' : 'translateY(32px)',
      transition: `opacity 0.6s ease ${delay}ms, transform 0.6s ease ${delay}ms`,
    }
  };
}

// ── ANIMATED STAT CARD ────────────────────────────────────
const StatCard = ({ label, val, sub, color, icon, animate }: any) => {
  const numericVal = typeof val === 'string'
    ? parseInt(val.replace(/\D/g, '')) || 0
    : val;
  const count = useCountUp(numericVal, 1400, animate);
  const fade  = useFadeUp(0);

  return (
    <div ref={fade.ref} style={fade.style} className={`stat-card-luxury ${color}`}>
      <div className="stat-icon-wrap">{icon}</div>
      <div className="stat-data">
        <span>{label}</span>
        <h2>
          {val === '—' || val === '...'
            ? val
            : animate
              ? count.toLocaleString()
              : val}
        </h2>
        <p style={{margin:0, fontSize:'0.55rem', opacity:0.7}}>{sub}</p>
      </div>
    </div>
  );
};

// ── FEED ITEM ─────────────────────────────────────────────
const FeedItem = ({ icon, title, desc, time }: any) => (
  <div className="feed-item">
    <div className="stat-icon-wrap" style={{width:30, height:30}}>{icon}</div>
    <div className="feed-text">
      <div style={{fontWeight:'bold', fontSize:'0.75rem'}}>{title}</div>
      <div style={{fontSize:'0.65rem', opacity:0.8}}>{desc}</div>
    </div>
    <span className="feed-time">{time}</span>
  </div>
);

// ── ACTION BUTTON ─────────────────────────────────────────
const ActionButton = ({ label, icon, onClick }: any) => (
  <div className="action-btn-luxury" onClick={onClick}>
    <div className="btn-icon">{icon}</div>
    <span>{label}</span>
  </div>
);

// ── MAIN DASHBOARD ────────────────────────────────────────
const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const [stats, setStats]         = useState<AdminStats | null>(null);
  const [players, setPlayers]     = useState<Player[]>([]);
  const [loading, setLoading]     = useState(true);
  const [chartType, setChartType] = useState<'line' | 'pie' | 'bar'>('line');
  const [animate, setAnimate]     = useState(false);

  // Fade up refs untuk setiap section
  const fadeHeader  = useFadeUp(0);
  const fadeStats   = useFadeUp(100);
  const fadeMiddle  = useFadeUp(200);
  const fadeBottom  = useFadeUp(350);

  useEffect(() => {
    Promise.all([getAdminStats(), getAllPlayers()])
      .then(([statsRes, playersRes]) => {
        setStats(statsRes.data);
        setPlayers(playersRes.data?.players || []);
        // Delay animasi counter setelah data masuk
        setTimeout(() => setAnimate(true), 300);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const totalPemain = stats?.total_players ?? 0;
  const totalTrivia = stats?.total_trivia  ?? 0;
  const totalBattle = stats?.total_battle  ?? 0;

  return (
    <div className="dashboard-container">

      {/* 1. HEADER */}
      <div ref={fadeHeader.ref} style={fadeHeader.style} className="welcome-header-luxury">
        {/* Background & Overlay */}
        <div className="banner-bg-layer" style={{ backgroundImage: `url(${bannerToba})` }}></div>
        <div className="banner-overlay"></div>

        {/* Konten Kiri: Mascot & Selamat Datang */}
        <div className="welcome-profile">
            <img src={logoToba} className="avatar-luxury" alt="admin mascot"/>
          <div className="text">
            <h2 style={{ margin: 0, fontSize: '1.6rem', color: 'var(--gold-toba)', textShadow: '2px 2px 4px rgba(0,0,0,0.8)' }}>
              Selamat datang, Admin!
            </h2>
            <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--text-cream)', opacity: 0.9 }}>
              Kelola konten dan data game TOBA dengan mudah.
            </p>
          </div>
        </div>
        {/* Konten Kanan: Notif & Admin Pill */}
        <div style={{display:'flex', gap:'20px', alignItems:'center'}}>
          <div style={{position:'relative', cursor:'pointer'}}>
            <Bell size={24} color="#D4AF37"/>
          </div>
          <div className="admin-pill">
            <img src={logoToba} alt="admin icon" />
            <div style={{display:'flex', flexDirection:'column'}}>
              <span style={{fontSize:'0.8rem', fontWeight:'bold'}}>AdminTOBA</span>
              <small style={{fontSize:'0.6rem', opacity:0.7}}>Super Admin</small>
            </div>
          </div>
        </div>
      </div>

      {/* 2. STATS GRID — dengan animated counter */}
      <div ref={fadeStats.ref} style={fadeStats.style} className="stats-grid-luxury">
        <StatCard
          label="TOTAL PEMAIN" val={loading ? '...' : totalPemain}
          sub="Data real dari database" color="green"
          icon={<Users size={20}/>} animate={animate && !loading}
        />
        <StatCard
          label="TOTAL LOKASI" val={10}
          sub="Semua lokasi aktif" color="blue"
          icon={<Map size={20}/>} animate={animate}
        />
        <StatCard
          label="TOTAL TRIVIA" val={loading ? '...' : totalTrivia}
          sub="Sesi trivia dimainkan" color="purple"
          icon={<HelpCircle size={20}/>} animate={animate && !loading}
        />
        <StatCard
          label="TOTAL BINTANG" val={0}
          sub="Dikumpulkan pemain" color="orange"
          icon={<Star size={20}/>} animate={animate}
        />
        <StatCard
          label="TOTAL BATTLE" val={loading ? '...' : totalBattle}
          sub="Battle selesai" color="red"
          icon={<Sword size={20}/>} animate={animate && !loading}
        />
      </div>

      {/* 3. CHART & ACTIVITY */}
      <div ref={fadeMiddle.ref} style={fadeMiddle.style} className="middle-row">
        <div className="wood-panel">
          <div className="panel-head">
            <h3>Statistik Pemain</h3>

            {/* Toggle Chart */}
            <div style={{display:'flex', alignItems:'center', gap:'6px'}}>
              {([
                { type: 'line', icon: <BarChart2 size={13}/>, label: 'Line' },
                { type: 'bar',  icon: <BarChart3 size={13}/>, label: 'Bar'  },
                { type: 'pie',  icon: <PieIcon   size={13}/>, label: 'Pie'  },
              ] as const).map(btn => (
                <button
                  key={btn.type}
                  onClick={() => setChartType(btn.type)}
                  style={{
                    display:'flex', alignItems:'center', gap:'4px',
                    padding:'4px 10px', borderRadius:'6px', cursor:'pointer',
                    fontSize:'0.7rem', fontWeight:'bold',
                    background: chartType === btn.type ? '#4D7023' : 'rgba(0,0,0,0.3)',
                    border:     chartType === btn.type ? '1px solid #D4AF37' : '1px solid #5C3B26',
                    color:      chartType === btn.type ? '#D4AF37' : '#F5F5DC',
                    transition: '0.2s',
                  }}
                >
                  {btn.icon} {btn.label}
                </button>
              ))}
              <select style={{background:'rgba(0,0,0,0.5)', color:'#fff', border:'1px solid #5C3B26', borderRadius:'5px', padding:'4px 8px', fontSize:'0.7rem'}}>
                <option>7 Hari Terakhir</option>
              </select>
            </div>
          </div>

          {/* Chart Area */}
          <div style={{height:250, width:'100%'}}>
            <ResponsiveContainer>
              {chartType === 'line' ? (
                <AreaChart data={dataChart}>
                  <defs>
                    <linearGradient id="colorVal" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%"  stopColor="#82ca9d" stopOpacity={0.35}/>
                      <stop offset="95%" stopColor="#82ca9d" stopOpacity={0}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" vertical={false}/>
                  <XAxis dataKey="name" stroke="#F5F5DC" fontSize={10} tickLine={false} axisLine={false}/>
                  <YAxis stroke="#F5F5DC" fontSize={10} tickLine={false} axisLine={false}/>
                  <Tooltip contentStyle={{backgroundColor:'#2C1810', border:'1px solid #D4AF37'}}/>
                  <Area
                    type="monotone" dataKey="val"
                    stroke="#82ca9d" strokeWidth={3}
                    fillOpacity={1} fill="url(#colorVal)"
                    isAnimationActive={true}
                    animationDuration={1200}
                    animationEasing="ease-out"
                  />
                </AreaChart>

              ) : chartType === 'bar' ? (
                <BarChart data={dataChart} barSize={32}>
                  <defs>
                    <linearGradient id="barGrad" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%"   stopColor="#D4AF37" stopOpacity={0.9}/>
                      <stop offset="100%" stopColor="#4D7023" stopOpacity={0.7}/>
                    </linearGradient>
                  </defs>
                  <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" vertical={false}/>
                  <XAxis dataKey="name" stroke="#F5F5DC" fontSize={10} tickLine={false} axisLine={false}/>
                  <YAxis stroke="#F5F5DC" fontSize={10} tickLine={false} axisLine={false}/>
                  <Tooltip contentStyle={{backgroundColor:'#2C1810', border:'1px solid #D4AF37'}}/>
                  <Bar
                    dataKey="val" fill="url(#barGrad)"
                    radius={[6,6,0,0]}
                    isAnimationActive={true}
                    animationDuration={1000}
                    animationEasing="ease-out"
                  />
                </BarChart>

              ) : (
                <PieChart>
                  <Pie
                    data={dataChart}
                    dataKey="val" nameKey="name"
                    cx="50%" cy="50%"
                    outerRadius={90} innerRadius={40}
                    paddingAngle={3}
                    isAnimationActive={true}
                    animationDuration={1200}
                    animationEasing="ease-out"
                    label={({ name, percent }) =>
                      `${name} ${((percent ?? 0) * 100).toFixed(0)}%`
                    }
                    labelLine={false}
                  >
                    {dataChart.map((_, i) => (
                      <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]}/>
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{backgroundColor:'#2C1810', border:'1px solid #D4AF37'}}
                    formatter={(v) => [Number(v).toLocaleString(), 'Pemain']}
                  />
                  <Legend formatter={v => (
                    <span style={{color:'#F5F5DC', fontSize:'0.72rem'}}>{v}</span>
                  )}/>
                </PieChart>
              )}
            </ResponsiveContainer>
          </div>
        </div>

        {/* PEMAIN TERBARU */}
        <div className="wood-panel">
          <div className="panel-head">
            <h3>Pemain Terbaru</h3>
            <button
              onClick={() => navigate('/players')}
              style={{background:'rgba(212,175,55,0.1)', border:'1px solid #D4AF37', color:'#D4AF37', fontSize:'0.6rem', borderRadius:'5px', padding:'2px 8px', cursor:'pointer'}}
            >
              Lihat Semua
            </button>
          </div>
          <div className="feed-list">
            {loading
              ? <p style={{opacity:0.5, fontSize:'0.8rem'}}>Memuat...</p>
              : players.slice(0, 5).map((p, i) => (
                  <div
                    key={p.id}
                    style={{
                      opacity: 0,
                      animation: `fadeSlideUp 0.5s ease forwards`,
                      animationDelay: `${i * 100}ms`,
                    }}
                  >
                    <FeedItem
                      icon={<Users color="#2196f3" size={16}/>}
                      title={p.name}
                      desc={'@' + p.username + ' • Level ' + p.level}
                      time={'⭐ ' + p.stars}
                    />
                  </div>
                ))
            }
          </div>
        </div>
      </div>

      {/* 4. TABLE & QUICK ACTIONS */}
      <div ref={fadeBottom.ref} style={fadeBottom.style} className="bottom-row">
        <div className="wood-panel">
          <div className="panel-head">
            <h3>Data Pemain</h3>
            <button
              onClick={() => navigate('/players')}
              style={{background:'rgba(212,175,55,0.1)', border:'1px solid #D4AF37', color:'#D4AF37', fontSize:'0.6rem', borderRadius:'5px', padding:'2px 8px', cursor:'pointer'}}
            >
              Kelola Pemain
            </button>
          </div>
          <table className="toba-table-luxury">
            <thead>
              <tr><th>Nama</th><th>Username</th><th>Level</th><th>Bintang</th><th>Status</th></tr>
            </thead>
            <tbody>
              {loading
                ? <tr><td colSpan={5} style={{textAlign:'center', opacity:0.5}}>Memuat...</td></tr>
                : players.slice(0, 5).map((p, i) => (
                    <tr
                      key={p.id}
                      style={{
                        opacity: 0,
                        animation: `fadeSlideUp 0.4s ease forwards`,
                        animationDelay: `${i * 80 + 200}ms`,
                      }}
                    >
                      <td>
                        <div style={{display:'flex', alignItems:'center', gap:'8px'}}>
                          <div style={{width:28, height:28, borderRadius:'50%', background:'#D4AF37', display:'flex', alignItems:'center', justifyContent:'center', fontWeight:'bold', color:'#2C1810', fontSize:'0.75rem', flexShrink:0}}>
                            {p.name.charAt(0).toUpperCase()}
                          </div>
                          {p.name}
                        </div>
                      </td>
                      <td style={{opacity:0.7}}>@{p.username}</td>
                      <td><span className="badge blue">Lv.{p.level}</span></td>
                      <td>⭐ {p.stars}</td>
                      <td><span className={`badge ${p.is_active ? 'green' : 'orange'}`}>{p.is_active ? 'Aktif' : 'Nonaktif'}</span></td>
                    </tr>
                  ))
              }
            </tbody>
          </table>
        </div>

        <div className="wood-panel">
          <h3 style={{fontSize:'0.9rem', color:'#D4AF37', marginBottom:'15px'}}>Aksi Cepat</h3>
          <div className="action-grid">
            {[
              { label:'Kelola Lokasi',  icon:<Map size={18}/>,           path:'/locations'    },
              { label:'Kelola Trivia',  icon:<HelpCircle size={18}/>,    path:'/trivia'       },
              { label:'Kelola Pemain',  icon:<Users size={18}/>,         path:'/players'      },
              { label:'Kelola Battle',  icon:<Sword size={18}/>,         path:'/battle'       },
              { label:'Achievement',    icon:<BarChartIcon size={18}/>,  path:'/achievements' },
              { label:'Pengaturan',     icon:<Settings size={18}/>,      path:'/settings'     },
            ].map((btn, i) => (
              <div
                key={i}
                className="action-btn-luxury"
                onClick={() => navigate(btn.path)}
                style={{
                  opacity: 0,
                  animation: `fadeSlideUp 0.4s ease forwards`,
                  animationDelay: `${i * 60 + 400}ms`,
                }}
              >
                <div className="btn-icon">{btn.icon}</div>
                <span>{btn.label}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;