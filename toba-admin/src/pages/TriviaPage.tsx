import React, { useEffect, useState } from 'react';
import { HelpCircle, Plus, Edit2, Trash2, RefreshCw, X, Check, ChevronDown, ChevronUp } from 'lucide-react';
import {
  adminGetTriviaCategories, adminCreateTriviaCategory,
  adminUpdateTriviaCategory, adminDeleteTriviaCategory,
  adminGetTriviaQuestions, adminCreateTriviaQuestion,
  adminUpdateTriviaQuestion, adminDeleteTriviaQuestion
} from '../services/api';
import type { TriviaCategory, TriviaQuestion, QuestionOption } from '../types';
import { styles } from '../styles/shared';

const QuestionModal = ({ catId, question, onClose, onSave }: {
  catId: number; question: TriviaQuestion | null;
  onClose: () => void; onSave: () => void;
}) => {
  const isEdit = !!question;
  const [q, setQ]   = useState(question?.question ?? '');
  const [exp, setExp] = useState(question?.explanation ?? '');
  const [opts, setOpts] = useState<QuestionOption[]>(
    question?.options ?? [
      { option_text: '', is_correct: true  },
      { option_text: '', is_correct: false },
      { option_text: '', is_correct: false },
      { option_text: '', is_correct: false },
    ]
  );
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const handleSave = async () => {
    if (!q || !exp) { setMsg('Pertanyaan dan penjelasan wajib.'); return; }
    const valid = opts.filter(o => o.option_text.trim());
    if (valid.length < 2) { setMsg('Minimal 2 pilihan.'); return; }
    if (!valid.some(o => o.is_correct)) { setMsg('Pilih jawaban benar.'); return; }
    setSaving(true);
    try {
      const payload = { question: q, explanation: exp, is_active: true, options: valid };
      if (isEdit) await adminUpdateTriviaQuestion(question!.id, payload);
      else await adminCreateTriviaQuestion(catId, payload);
      onSave();
    } catch { setMsg('Gagal menyimpan.'); }
    finally { setSaving(false); }
  };

  return (
    <div style={styles.overlay}>
      <div style={{...styles.modal, maxWidth: 600}}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>{isEdit ? 'Edit Soal Trivia' : 'Tambah Soal Trivia'}</h3>
          <button onClick={onClose} style={styles.closeBtn}><X size={18}/></button>
        </div>
        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}
          <div style={styles.field}>
            <label style={styles.label}>Pertanyaan *</label>
            <textarea style={{...styles.input, height: 70, resize: 'vertical'}} value={q} onChange={e => setQ(e.target.value)} placeholder="Tulis pertanyaan..."/>
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Penjelasan *</label>
            <textarea style={{...styles.input, height: 60, resize: 'vertical'}} value={exp} onChange={e => setExp(e.target.value)} placeholder="Penjelasan jawaban..."/>
          </div>
          <label style={styles.label}>Pilihan Jawaban</label>
          {opts.map((opt, i) => (
            <div key={i} style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 8 }}>
              <span style={{ color: '#D4AF37', fontWeight: 'bold', width: 20 }}>{String.fromCharCode(65+i)}.</span>
              <input type="radio" name="correct" checked={opt.is_correct} onChange={() => setOpts(p => p.map((o, idx) => ({...o, is_correct: idx === i})))} style={{ accentColor: '#4CAF50' }}/>
              <input style={{...styles.input, flex: 1, margin: 0}} value={opt.option_text} onChange={e => setOpts(p => p.map((o, idx) => idx === i ? {...o, option_text: e.target.value} : o))} placeholder={`Pilihan ${String.fromCharCode(65+i)}`}/>
              {opt.is_correct && <span style={{ color: '#4CAF50', fontSize: 12, whiteSpace: 'nowrap' }}>✓ Benar</span>}
            </div>
          ))}
        </div>
        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}><Check size={14}/> {saving ? 'Menyimpan...' : 'Simpan'}</button>
        </div>
      </div>
    </div>
  );
};

const CategoryModal = ({ cat, onClose, onSave }: {
  cat: TriviaCategory | null; onClose: () => void; onSave: () => void;
}) => {
  const isEdit = !!cat;
  const [form, setForm] = useState({
    name: cat?.name ?? '', description: cat?.description ?? '',
    question_count: cat?.question_count ?? 10, display_order: cat?.display_order ?? 1,
    is_active: cat?.is_active ?? true,
  });
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const handleSave = async () => {
    if (!form.name) { setMsg('Nama kategori wajib diisi.'); return; }
    setSaving(true);
    try {
      if (isEdit) await adminUpdateTriviaCategory(cat!.id, form);
      else await adminCreateTriviaCategory(form);
      onSave();
    } catch { setMsg('Gagal menyimpan.'); }
    finally { setSaving(false); }
  };

  return (
    <div style={styles.overlay}>
      <div style={styles.modal}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>{isEdit ? 'Edit Kategori' : 'Tambah Kategori'}</h3>
          <button onClick={onClose} style={styles.closeBtn}><X size={18}/></button>
        </div>
        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}
          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Nama Kategori *</label>
              <input style={styles.input} value={form.name} onChange={e => setForm(f => ({...f, name: e.target.value}))} placeholder="Makanan Tradisional"/>
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Urutan Tampil</label>
              <input style={styles.input} type="number" value={form.display_order} onChange={e => setForm(f => ({...f, display_order: +e.target.value}))}/>
            </div>
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Deskripsi</label>
            <input style={styles.input} value={form.description} onChange={e => setForm(f => ({...f, description: e.target.value}))} placeholder="Deskripsi kategori..."/>
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Jumlah Soal per Sesi</label>
            <input style={styles.input} type="number" value={form.question_count} onChange={e => setForm(f => ({...f, question_count: +e.target.value}))}/>
          </div>
          {isEdit && (
            <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
              <input type="checkbox" checked={form.is_active} onChange={e => setForm(f => ({...f, is_active: e.target.checked}))} id="catActive"/>
              <label htmlFor="catActive" style={styles.label}>Aktif</label>
            </div>
          )}
        </div>
        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}><Check size={14}/> {saving ? 'Menyimpan...' : 'Simpan'}</button>
        </div>
      </div>
    </div>
  );
};

const TriviaPage: React.FC = () => {
  const [categories, setCategories] = useState<TriviaCategory[]>([]);
  const [loading, setLoading]       = useState(true);
  const [selected, setSelected]     = useState<TriviaCategory | null>(null);
  const [questions, setQuestions]   = useState<TriviaQuestion[]>([]);
  const [qLoading, setQLoading]     = useState(false);
  const [showCatModal, setShowCatModal] = useState(false);
  const [editCat, setEditCat]       = useState<TriviaCategory | null>(null);
  const [showQModal, setShowQModal] = useState(false);
  const [editQ, setEditQ]           = useState<TriviaQuestion | null>(null);

  const fetchCategories = () => {
    setLoading(true);
    adminGetTriviaCategories().then(r => setCategories(r.data || [])).finally(() => setLoading(false));
  };

  const loadQuestions = async (cat: TriviaCategory) => {
    setSelected(cat); setQLoading(true);
    const r = await adminGetTriviaQuestions(cat.id);
    setQuestions(r.data || []);
    setQLoading(false);
  };

  const handleDeleteCat = async (id: number) => {
    if (!confirm('Hapus kategori ini? Semua soal di kategori ini juga akan terhapus!')) return;
    await adminDeleteTriviaCategory(id);
    if (selected?.id === id) setSelected(null);
    fetchCategories();
  };

  const handleDeleteQ = async (id: number) => {
    if (!confirm('Hapus soal ini?')) return;
    await adminDeleteTriviaQuestion(id);
    if (selected) loadQuestions(selected);
  };

  useEffect(() => { fetchCategories(); }, []);

  return (
    <div className="dashboard-container">
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><HelpCircle size={24}/></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Kelola Trivia Budaya</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>{categories.length} kategori • kelola soal per kategori</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={fetchCategories} style={styles.btnSecondary}><RefreshCw size={14}/> Refresh</button>
          <button onClick={() => { setEditCat(null); setShowCatModal(true); }} style={styles.btnPrimary}>
            <Plus size={14}/> Tambah Kategori
          </button>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '300px 1fr', gap: 20, alignItems: 'start' }}>
        {/* Sidebar Kategori */}
        <div className="wood-panel">
          <div className="panel-head">
            <h3>Kategori</h3>
            <span style={{ fontSize: '0.75rem', opacity: 0.5 }}>{categories.length} total</span>
          </div>
          {loading ? (
            <p style={{ opacity: 0.5, fontSize: '0.8rem' }}>Memuat...</p>
          ) : categories.map(cat => (
            <div key={cat.id} style={{ marginBottom: 6 }}>
              <div
                onClick={() => loadQuestions(cat)}
                style={{
                  padding: '10px 12px', borderRadius: 8, cursor: 'pointer',
                  background: selected?.id === cat.id ? '#4D7023' : 'rgba(0,0,0,0.2)',
                  border: selected?.id === cat.id ? '1px solid #D4AF37' : '1px solid transparent',
                  transition: '0.2s', display: 'flex', alignItems: 'center', gap: 8
                }}
              >
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 'bold', fontSize: '0.85rem' }}>{cat.name}</div>
                  <div style={{ fontSize: '0.7rem', opacity: 0.6 }}>{cat.question_count} soal/sesi • {cat.is_active ? 'Aktif' : 'Nonaktif'}</div>
                </div>
                <div style={{ display: 'flex', gap: 4 }}>
                  <button onClick={e => { e.stopPropagation(); setEditCat(cat); setShowCatModal(true); }} style={{ background: 'none', border: 'none', color: '#D4AF37', cursor: 'pointer' }}>
                    <Edit2 size={13}/>
                  </button>
                  <button onClick={e => { e.stopPropagation(); handleDeleteCat(cat.id); }} style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer' }}>
                    <Trash2 size={13}/>
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Panel Soal */}
        <div className="wood-panel">
          {!selected ? (
            <div style={{ textAlign: 'center', padding: 60, opacity: 0.4 }}>
              <HelpCircle size={48} style={{ margin: '0 auto 12px', display: 'block' }}/>
              <p>Pilih kategori untuk kelola soal</p>
            </div>
          ) : (
            <>
              <div className="panel-head">
                <h3>{selected.name}</h3>
                <div style={{ display: 'flex', gap: 8 }}>
                  <span style={{ fontSize: '0.75rem', opacity: 0.6, alignSelf: 'center' }}>{questions.length} soal</span>
                  <button
                    onClick={(e) => {
                      e.stopPropagation(); // ← tambah ini
                      setEditQ(null);
                      setShowQModal(true);
                    }}
                    style={styles.btnPrimary}>
                    <Plus size={13}/> Tambah Soal
                  </button>
                </div>
              </div>
              {qLoading ? (
                <p style={{ opacity: 0.5, textAlign: 'center', padding: 20 }}>Memuat soal...</p>
              ) : questions.length === 0 ? (
                <p style={{ opacity: 0.4, textAlign: 'center', padding: 20 }}>Belum ada soal. Tambah sekarang!</p>
              ) : questions.map((q, i) => (
                <div key={q.id} style={{ marginBottom: 10, background: 'rgba(0,0,0,0.2)', borderRadius: 8, overflow: 'hidden', border: '1px solid rgba(212,175,55,0.1)' }}>
                  <div style={{ padding: '12px 16px', display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 8 }}>
                    <div style={{ flex: 1 }}>
                      <span style={{ fontSize: '0.7rem', color: '#D4AF37', marginRight: 8 }}>#{i+1}</span>
                      <span style={{ fontSize: '0.85rem' }}>{q.question}</span>
                    </div>
                    <div style={{ display: 'flex', gap: 4, flexShrink: 0 }}>
                      <span className={`badge ${q.is_active ? 'green' : 'orange'}`} style={{ fontSize: 9 }}>{q.is_active ? 'Aktif' : 'Nonaktif'}</span>
                      <button onClick={() => { setEditQ(q); setShowQModal(true); }} style={{ background: 'none', border: 'none', color: '#D4AF37', cursor: 'pointer' }}><Edit2 size={14}/></button>
                      <button onClick={() => handleDeleteQ(q.id)} style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer' }}><Trash2 size={14}/></button>
                    </div>
                  </div>
                  <div style={{ padding: '0 16px 12px', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6 }}>
                    {q.options?.map((opt, oi) => (
                      <div key={oi} style={{ padding: '5px 10px', borderRadius: 5, fontSize: '0.78rem', background: opt.is_correct ? 'rgba(76,175,80,0.15)' : 'rgba(0,0,0,0.2)', border: opt.is_correct ? '1px solid #4CAF50' : '1px solid rgba(255,255,255,0.05)' }}>
                        {String.fromCharCode(65+oi)}. {opt.option_text}
                        {opt.is_correct && <span style={{ color: '#4CAF50', marginLeft: 4 }}>✓</span>}
                      </div>
                    ))}
                  </div>
                  <div style={{ padding: '0 16px 12px', fontSize: '0.75rem', opacity: 0.7, borderLeft: '3px solid #D4AF37', marginLeft: 16, marginRight: 16, paddingLeft: 10 }}>
                    💡 {q.explanation}
                  </div>
                </div>
              ))}
            </>
          )}
        </div>
      </div>

      {showCatModal && <CategoryModal cat={editCat} onClose={() => setShowCatModal(false)} onSave={() => { setShowCatModal(false); fetchCategories(); }}/>}
      {showQModal && selected && (
        <QuestionModal
          key={`qmodal-${selected.id}-${editQ?.id ?? 'new'}`}
          catId={selected.id}
          question={editQ}
          onClose={() => { setShowQModal(false); setEditQ(null); }}
          onSave={() => {
            setShowQModal(false);
            setEditQ(null);
            if (selected) loadQuestions(selected);
          }}
        />
      )}
    </div>
  );
};

export default TriviaPage;