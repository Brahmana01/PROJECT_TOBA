import React, { useEffect, useState, useRef } from 'react';
import { MapPin, Plus, Edit2, Trash2, RefreshCw, ChevronDown, ChevronUp, X, Check, Upload } from 'lucide-react';
import {
  adminGetLocations, adminCreateLocation, adminUpdateLocation,
  adminDeleteLocation, adminUploadLocationImage,
  adminGetFacts, adminCreateFact, adminDeleteFact,
  adminGetLocQuestions, adminCreateLocQuestion,
  adminUpdateLocQuestion, adminDeleteLocQuestion
} from '../services/api';
import type { Location, LocationFact, LocationQuestion, QuestionOption } from '../types';
import { styles } from '../styles/shared';

// ── MODAL FORM LOKASI ─────────────────────────────────────
const LocationModal = ({ loc, onClose, onSave }: {
  loc: Location | null; onClose: () => void; onSave: () => void;
}) => {
  const isEdit = !!loc;
  const fileRef = useRef<HTMLInputElement>(null);
  const [form, setForm] = useState({
    name: loc?.name ?? '', description: loc?.description ?? '',
    image: loc?.image ?? '', reward_xp: loc?.reward_xp ?? 100,
    reward_stars: loc?.reward_stars ?? 3, question_count: loc?.question_count ?? 5,
    minimum_correct: loc?.minimum_correct ?? 3, level_order: loc?.level_order ?? 1,
    is_active: loc?.is_active ?? true,
  });
  const [uploading, setUploading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');
  const [activeLoc, setActiveLoc] = useState<Location | null>(null);

  const handleUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (!file) return;

      // Compress sebelum upload
      const compressed = await compressImage(file, 800, 0.8);
      setUploading(true);
      try {
          const res = await adminUploadLocationImage(compressed);
          setForm(f => ({ ...f, image: res.data.filename }));
          setMsg('Gambar berhasil diupload!');
      } catch {
          setMsg('Gagal upload gambar.');
      } finally {
          setUploading(false);
      }
  };


  //helper function ini di luar component:
  async function compressImage(file: File, maxWidth: number,
      quality: number): Promise<File> {
      return new Promise((resolve) => {
          const reader = new FileReader();
          reader.readAsDataURL(file);
          reader.onload = (e) => {
              const img = new Image();
              img.src = e.target?.result as string;
              img.onload = () => {
                  const canvas = document.createElement('canvas');
                  let { width, height } = img;

                  if (width > maxWidth) {
                      height = (height * maxWidth) / width;
                      width = maxWidth;
                  }

                  canvas.width = width;
                  canvas.height = height;
                  const ctx = canvas.getContext('2d')!;
                  ctx.drawImage(img, 0, 0, width, height);

                  canvas.toBlob((blob) => {
                      if (blob) {
                          resolve(new File([blob], file.name,
                              { type: 'image/jpeg' }));
                      } else {
                          resolve(file); // fallback
                      }
                  }, 'image/jpeg', quality);
              };
          };
      });
  }  

  const handleSave = async () => {
    if (!form.name || !form.description) { setMsg('Nama dan deskripsi wajib diisi.'); return; }
    setSaving(true);
    try {
      if (isEdit) await adminUpdateLocation(loc!.id, form);
      else await adminCreateLocation(form);
      onSave();
    } catch { setMsg('Gagal menyimpan.'); }
    finally { setSaving(false); }
  };

  return (
    <div style={styles.overlay}>
      <div style={styles.modal}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>{isEdit ? 'Edit Lokasi' : 'Tambah Lokasi'}</h3>
          <button onClick={onClose} style={styles.closeBtn}><X size={18}/></button>
        </div>
        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}

          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Nama Lokasi *</label>
              <input style={styles.input} value={form.name} onChange={e => setForm(f => ({...f, name: e.target.value}))} placeholder="IT Del"/>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Urutan Level *</label>
              <input style={styles.input} type="number" value={form.level_order} onChange={e => setForm(f => ({...f, level_order: +e.target.value}))}/>
            </div>
          </div>

          <div style={styles.field}>
            <label style={styles.label}>Deskripsi *</label>
            <textarea style={{...styles.input, height: 80, resize: 'vertical'}} value={form.description} onChange={e => setForm(f => ({...f, description: e.target.value}))} placeholder="Deskripsi lokasi..."/>
          </div>

          <div style={styles.field}>
            <label style={styles.label}>Gambar Lokasi (1 gambar)</label>
            <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
              <input style={{...styles.input, flex: 1}} value={form.image} onChange={e => setForm(f => ({...f, image: e.target.value}))} placeholder="nama-file.jpg atau upload"/>
              <button onClick={() => fileRef.current?.click()} style={styles.btnSecondary} disabled={uploading}>
                <Upload size={14}/> {uploading ? 'Mengupload...' : 'Upload'}
              </button>
              <input ref={fileRef} type="file" accept="image/*" style={{ display: 'none' }} onChange={handleUpload}/>
            </div>
            {form.image && <small style={{ color: '#82ca9d', marginTop: 4, display: 'block' }}>✓ {form.image}</small>}
          </div>

          <div style={styles.grid4}>
            <div style={styles.field}>
              <label style={styles.label}>Reward XP</label>
              <input style={styles.input} type="number" value={form.reward_xp} onChange={e => setForm(f => ({...f, reward_xp: +e.target.value}))}/>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Reward Bintang</label>
              <input style={styles.input} type="number" value={form.reward_stars} onChange={e => setForm(f => ({...f, reward_stars: +e.target.value}))}/>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Jumlah Soal</label>
              <input style={styles.input} type="number" value={form.question_count} onChange={e => setForm(f => ({...f, question_count: +e.target.value}))}/>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Min. Benar</label>
              <input style={styles.input} type="number" value={form.minimum_correct} onChange={e => setForm(f => ({...f, minimum_correct: +e.target.value}))}/>
            </div>
          </div>

          {isEdit && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <input type="checkbox" checked={form.is_active} onChange={e => setForm(f => ({...f, is_active: e.target.checked}))} id="isActive"/>
              <label htmlFor="isActive" style={styles.label}>Aktif</label>
            </div>
          )}
        </div>
        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}>
            <Check size={14}/> {saving ? 'Menyimpan...' : 'Simpan'}
          </button>
        </div>
      </div>
    </div>
  );
};

// ── MODAL SOAL ────────────────────────────────────────────
const QuestionModal = ({ locId, question, onClose, onSave }: {
  locId: number; question: LocationQuestion | null;
  onClose: () => void; onSave: () => void;
}) => {
  const isEdit = !!question;
  const [q, setQ] = useState(question?.question ?? '');
  const [exp, setExp] = useState(question?.explanation ?? '');
  const [opts, setOpts] = useState<QuestionOption[]>(
    question?.options ?? [
      { option_text: '', is_correct: true },
      { option_text: '', is_correct: false },
      { option_text: '', is_correct: false },
      { option_text: '', is_correct: false },
    ]
  );
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const setCorrect = (i: number) => setOpts(prev => prev.map((o, idx) => ({...o, is_correct: idx === i})));

  const handleSave = async () => {
    if (!q || !exp) { setMsg('Pertanyaan dan penjelasan wajib diisi.'); return; }
    const hasCorrect = opts.some(o => o.is_correct && o.option_text);
    if (!hasCorrect) { setMsg('Pilih 1 jawaban benar.'); return; }
    const validOpts = opts.filter(o => o.option_text.trim());
    if (validOpts.length < 2) { setMsg('Minimal 2 pilihan jawaban.'); return; }
    setSaving(true);
    try {
      const payload = { question: q, explanation: exp, is_active: true, options: validOpts };
      if (isEdit) await adminUpdateLocQuestion(question!.id, payload);
      else await adminCreateLocQuestion(locId, payload);
      onSave();
    } catch { setMsg('Gagal menyimpan.'); }
    finally { setSaving(false); }
  };

  return (
    <div style={styles.overlay}>
      <div style={{...styles.modal, maxWidth: 600}}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>{isEdit ? 'Edit Soal' : 'Tambah Soal'}</h3>
          <button onClick={onClose} style={styles.closeBtn}><X size={18}/></button>
        </div>
        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}
          <div style={styles.field}>
            <label style={styles.label}>Pertanyaan *</label>
            <textarea style={{...styles.input, height: 70, resize: 'vertical'}} value={q} onChange={e => setQ(e.target.value)} placeholder="Tulis pertanyaan..."/>
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Penjelasan Jawaban *</label>
            <textarea style={{...styles.input, height: 60, resize: 'vertical'}} value={exp} onChange={e => setExp(e.target.value)} placeholder="Penjelasan setelah jawab..."/>
          </div>
          <label style={styles.label}>Pilihan Jawaban (klik radio = jawaban benar)</label>
          {opts.map((opt, i) => (
            <div key={i} style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 8 }}>
              <span style={{ color: '#D4AF37', fontWeight: 'bold', width: 20 }}>{String.fromCharCode(65+i)}.</span>
              <input
                type="radio" name="correct" checked={opt.is_correct}
                onChange={() => setCorrect(i)}
                style={{ accentColor: '#4CAF50', cursor: 'pointer' }}
              />
              <input
                style={{...styles.input, flex: 1, margin: 0}}
                value={opt.option_text}
                onChange={e => setOpts(prev => prev.map((o, idx) => idx === i ? {...o, option_text: e.target.value} : o))}
                placeholder={`Pilihan ${String.fromCharCode(65+i)}`}
              />
              {opt.is_correct && <span style={{ color: '#4CAF50', fontSize: 12 }}>✓ Benar</span>}
            </div>
          ))}
        </div>
        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}>
            <Check size={14}/> {saving ? 'Menyimpan...' : 'Simpan'}
          </button>
        </div>
      </div>
    </div>
  );
};

// ── MAIN PAGE ─────────────────────────────────────────────
const LocationsPage: React.FC = () => {
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading]     = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editLoc, setEditLoc]     = useState<Location | null>(null);
  const [expanded, setExpanded]   = useState<number | null>(null);
  const [activeLoc, setActiveLoc] = useState<Location | null>(null);

  // Sub-data per lokasi
  const [facts, setFacts]       = useState<LocationFact[]>([]);
  const [questions, setQuestions] = useState<LocationQuestion[]>([]);
  const [newFact, setNewFact]   = useState('');
  const [showQModal, setShowQModal] = useState(false);
  const [editQ, setEditQ]       = useState<LocationQuestion | null>(null);
  const [subLoading, setSubLoading] = useState(false);

  const fetch = () => {
    setLoading(true);
    adminGetLocations().then(r => setLocations(r.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { fetch(); }, []);

  const handleDelete = async (id: number) => {
    if (!confirm('Hapus lokasi ini?')) return;
    await adminDeleteLocation(id);
    fetch();
  };

  const openExpand = async (loc: Location) => {
    if (expanded === loc.id) {
      setExpanded(null);
      setActiveLoc(null); // ← tambah
      return;
    }
    setExpanded(loc.id);
    setActiveLoc(loc);   // ← tambah
    setSubLoading(true);
    const [factsRes, qRes] = await Promise.all([
      adminGetFacts(loc.id), adminGetLocQuestions(loc.id)
    ]);
    setFacts(factsRes.data || []);
    setQuestions(qRes.data || []);
    setSubLoading(false);
  };

  const handleAddFact = async (locId: number) => {
    if (!newFact.trim()) return;
    await adminCreateFact(locId, newFact.trim());
    setNewFact('');
    const res = await adminGetFacts(locId);
    setFacts(res.data || []);
  };

  const handleDeleteFact = async (id: number, locId: number) => {
    await adminDeleteFact(id);
    const res = await adminGetFacts(locId);
    setFacts(res.data || []);
  };

  const handleDeleteQ = async (id: number, locId: number) => {
    if (!confirm('Hapus soal ini?')) return;
    await adminDeleteLocQuestion(id);
    const res = await adminGetLocQuestions(locId);
    setQuestions(res.data || []);
  };

  return (
    <div className="dashboard-container">
      {/* Header */}
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><MapPin size={24}/></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Kelola Lokasi Jelajah</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>{locations.length} lokasi • termasuk soal & fakta budaya</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={fetch} style={styles.btnSecondary}><RefreshCw size={14}/> Refresh</button>
          <button onClick={() => { setEditLoc(null); setShowModal(true); }} style={styles.btnPrimary}>
            <Plus size={14}/> Tambah Lokasi
          </button>
        </div>
      </div>

      {/* Daftar Lokasi */}
      {loading ? (
        <div className="wood-panel" style={{ textAlign: 'center', padding: 40, opacity: 0.5 }}>Memuat...</div>
      ) : locations.map(loc => (
        <div key={loc.id} className="wood-panel" style={{ padding: 0, overflow: 'hidden' }}>
          {/* Row lokasi */}
          <div style={{ display: 'flex', alignItems: 'center', padding: '16px 20px', gap: 16 }}>
            {/* Nomor urut */}
            <div style={{ width: 40, height: 40, borderRadius: '50%', background: '#D4AF37', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 'bold', color: '#2C1810', flexShrink: 0 }}>
              {loc.level_order}
            </div>

            {/* Info */}
            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 'bold', fontSize: '1rem', color: '#D4AF37' }}>{loc.name}</div>
              <div style={{ fontSize: '0.75rem', opacity: 0.7, marginTop: 2 }}>
                {loc.description?.substring(0, 80)}...
              </div>
              <div style={{ display: 'flex', gap: 8, marginTop: 6, flexWrap: 'wrap' }}>
                <span className="badge blue">XP +{loc.reward_xp}</span>
                <span className="badge green">⭐ +{loc.reward_stars}</span>
                <span className="badge" style={{ background: '#5C3B26' }}>📝 {loc.question_count} soal</span>
                <span className={`badge ${loc.is_active ? 'green' : 'orange'}`}>{loc.is_active ? 'Aktif' : 'Nonaktif'}</span>
                {loc.image && <span className="badge" style={{ background: '#1e88e5' }}>🖼 {loc.image}</span>}
              </div>
            </div>

            {/* Aksi */}
            <div style={{ display: 'flex', gap: 8, flexShrink: 0 }}>
              <button onClick={() => openExpand(loc)} style={styles.btnIcon} title="Kelola fakta & soal">
                {expanded === loc.id ? <ChevronUp size={16}/> : <ChevronDown size={16}/>}
              </button>
              <button onClick={() => { setEditLoc(loc); setShowModal(true); }} style={styles.btnIcon} title="Edit lokasi">
                <Edit2 size={16}/>
              </button>
              <button onClick={() => handleDelete(loc.id)} style={{...styles.btnIcon, color: '#F44336'}} title="Hapus lokasi">
                <Trash2 size={16}/>
              </button>
            </div>
          </div>

          {/* Panel expand — fakta & soal */}
          {expanded === loc.id && (
            <div style={{ borderTop: '1px solid rgba(212,175,55,0.2)', padding: '16px 20px', background: 'rgba(0,0,0,0.2)' }}>
              {subLoading ? (
                <p style={{ opacity: 0.5 }}>Memuat data...</p>
              ) : (
                <div style={styles.grid2}>
                  {/* FAKTA BUDAYA */}
                  <div>
                    <div style={{ fontWeight: 'bold', color: '#D4AF37', marginBottom: 10, fontSize: '0.9rem' }}>
                      💡 Fakta Budaya ({facts.length})
                    </div>
                    <div style={{ display: 'flex', gap: 6, marginBottom: 10 }}>
                      <input
                        style={{...styles.input, flex: 1, margin: 0, fontSize: '0.8rem'}}
                        value={newFact}
                        onChange={e => setNewFact(e.target.value)}
                        placeholder="Tambah fakta baru..."
                        onKeyDown={e => e.key === 'Enter' && handleAddFact(loc.id)}
                      />
                      <button onClick={() => handleAddFact(loc.id)} style={styles.btnPrimary}>
                        <Plus size={14}/>
                      </button>
                    </div>
                    {facts.length === 0 ? (
                      <p style={{ opacity: 0.4, fontSize: '0.8rem' }}>Belum ada fakta.</p>
                    ) : facts.map(f => (
                      <div key={f.id} style={{ display: 'flex', gap: 8, alignItems: 'flex-start', marginBottom: 6, background: 'rgba(0,0,0,0.2)', padding: '6px 10px', borderRadius: 6 }}>
                        <span style={{ flex: 1, fontSize: '0.78rem', lineHeight: 1.5 }}>{f.fact}</span>
                        <button onClick={() => handleDeleteFact(f.id, loc.id)} style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer', padding: 2, flexShrink: 0 }}>
                          <Trash2 size={13}/>
                        </button>
                      </div>
                    ))}
                  </div>

                  {/* SOAL JELAJAH */}
                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
                      <span style={{ fontWeight: 'bold', color: '#D4AF37', fontSize: '0.9rem' }}>
                        📝 Bank Soal ({questions.length})
                      </span>
                      <button onClick={() => { setEditQ(null); setShowQModal(true); }} style={styles.btnPrimary}>
                        <Plus size={13}/> Tambah Soal
                      </button>
                    </div>
                    {questions.length === 0 ? (
                      <p style={{ opacity: 0.4, fontSize: '0.8rem' }}>Belum ada soal.</p>
                    ) : questions.map((q, i) => (
                      <div key={q.id} style={{ background: 'rgba(0,0,0,0.2)', padding: '8px 12px', borderRadius: 6, marginBottom: 6 }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                          <span style={{ fontSize: '0.8rem', flex: 1 }}>
                            <span style={{ color: '#D4AF37', marginRight: 6 }}>#{i+1}</span>
                            {q.question}
                          </span>
                          <div style={{ display: 'flex', gap: 4, flexShrink: 0, marginLeft: 8 }}>
                            <button onClick={() => { setEditQ(q); setShowQModal(true); }} style={{ background: 'none', border: 'none', color: '#D4AF37', cursor: 'pointer' }}>
                              <Edit2 size={13}/>
                            </button>
                            <button onClick={() => handleDeleteQ(q.id, loc.id)} style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer' }}>
                              <Trash2 size={13}/>
                            </button>
                          </div>
                        </div>
                        <div style={{ display: 'flex', gap: 4, flexWrap: 'wrap', marginTop: 4 }}>
                          {q.options?.map((opt, oi) => (
                            <span key={oi} style={{ fontSize: '0.7rem', padding: '1px 6px', borderRadius: 3, background: opt.is_correct ? 'rgba(76,175,80,0.3)' : 'rgba(255,255,255,0.05)', border: opt.is_correct ? '1px solid #4CAF50' : '1px solid transparent' }}>
                              {String.fromCharCode(65+oi)}. {opt.option_text}
                            </span>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      ))}

      {showModal && (
        <LocationModal loc={editLoc} onClose={() => setShowModal(false)} onSave={() => { setShowModal(false); fetch(); }}/>
      )}
      {showQModal && activeLoc !== null && (
        <QuestionModal
          locId={activeLoc.id}
          question={editQ}
          onClose={() => setShowQModal(false)}
          onSave={async () => {
            setShowQModal(false);
            if (activeLoc) {
              const res = await adminGetLocQuestions(activeLoc.id);
              setQuestions(res.data || []);
            }
          }}
        />
      )}
    </div>
  );
};

export default LocationsPage;