import React, { useEffect, useState } from 'react';
import { HelpCircle, Plus, Edit2, Trash2, RefreshCw, X, Check } from 'lucide-react';
import {
  adminGetTriviaCategories,
  adminUpdateTriviaCategory,
  adminGetTriviaQuestions,
  adminCreateTriviaQuestion,
  adminUpdateTriviaQuestion,
  adminDeleteTriviaQuestion,
} from '../services/api';
import type { TriviaCategory, TriviaQuestion, QuestionOption } from '../types';
import { styles } from '../styles/shared';

const SESSION_QUESTION_COUNT = 10;

type ApiError = {
  response?: {
    data?: {
      message?: string;
    };
  };
};

function getApiError(error: unknown, fallback: string): string {
  return (error as ApiError)?.response?.data?.message || fallback;
}

const createEmptyOptions = (): QuestionOption[] => [
  { option_text: '', is_correct: true },
  { option_text: '', is_correct: false },
  { option_text: '', is_correct: false },
  { option_text: '', is_correct: false },
];

// -----------------------------------------------------------------------------
// MODAL TAMBAH / EDIT SOAL TRIVIA
// -----------------------------------------------------------------------------
type QuestionModalProps = {
  catId: number;
  question: TriviaQuestion | null;
  onClose: () => void;
  onSave: () => Promise<void> | void;
};

const QuestionModal: React.FC<QuestionModalProps> = ({
  catId,
  question,
  onClose,
  onSave,
}) => {
  const isEdit = Boolean(question);
  const [q, setQ] = useState(question?.question ?? '');
  const [exp, setExp] = useState(question?.explanation ?? '');
  const [options, setOptions] = useState<QuestionOption[]>(() => {
    if (question?.options?.length === 4) {
      return question.options.map((option) => ({ ...option }));
    }
    return createEmptyOptions();
  });
  const [isActive, setIsActive] = useState(question?.is_active ?? true);
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const setCorrectOption = (selectedIndex: number) => {
    setOptions((current) => current.map((option, index) => ({
      ...option,
      is_correct: index === selectedIndex,
    })));
  };

  const handleSave = async () => {
    if (!q.trim() || !exp.trim()) {
      setMsg('Pertanyaan dan penjelasan wajib diisi.');
      return;
    }
    if (options.length !== 4 || options.some((option) => !option.option_text.trim())) {
      setMsg('Isi tepat 4 pilihan jawaban. Tidak boleh ada pilihan kosong.');
      return;
    }
    if (options.filter((option) => option.is_correct).length !== 1) {
      setMsg('Pilih tepat 1 jawaban benar.');
      return;
    }

    setSaving(true);
    setMsg('');
    try {
      const payload = {
        question: q.trim(),
        explanation: exp.trim(),
        is_active: isActive,
        options: options.map((option) => ({
          id: option.id,
          option_text: option.option_text.trim(),
          is_correct: option.is_correct,
        })),
      };

      if (isEdit && question) {
        await adminUpdateTriviaQuestion(question.id, payload);
      } else {
        await adminCreateTriviaQuestion(catId, payload);
      }
      await onSave();
    } catch (error) {
      setMsg(getApiError(error, 'Gagal menyimpan soal Trivia.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div style={styles.overlay}>
      <div style={{ ...styles.modal, maxWidth: 620 }}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>
            {isEdit ? 'Edit Soal Trivia' : 'Tambah Soal Trivia'}
          </h3>
          <button onClick={onClose} style={styles.closeBtn} aria-label="Tutup">
            <X size={18} />
          </button>
        </div>

        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}
          <div style={styles.field}>
            <label style={styles.label}>Pertanyaan *</label>
            <textarea
              style={{ ...styles.input, height: 75, resize: 'vertical' }}
              value={q}
              onChange={(event) => setQ(event.target.value)}
              placeholder="Tulis pertanyaan..."
            />
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Penjelasan *</label>
            <textarea
              style={{ ...styles.input, height: 65, resize: 'vertical' }}
              value={exp}
              onChange={(event) => setExp(event.target.value)}
              placeholder="Penjelasan jawaban..."
            />
          </div>

          <label style={styles.label}>Empat pilihan jawaban (pilih satu yang benar)</label>
          {options.map((option, index) => (
            <div key={index} style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 8 }}>
              <span style={{ color: '#D4AF37', fontWeight: 'bold', width: 20 }}>
                {String.fromCharCode(65 + index)}.
              </span>
              <input
                type="radio"
                name="trivia-correct-option"
                checked={option.is_correct}
                onChange={() => setCorrectOption(index)}
                aria-label={`Jadikan pilihan ${String.fromCharCode(65 + index)} jawaban benar`}
              />
              <input
                style={{ ...styles.input, flex: 1, margin: 0 }}
                value={option.option_text}
                onChange={(event) => setOptions((current) => current.map((item, itemIndex) =>
                  itemIndex === index ? { ...item, option_text: event.target.value } : item,
                ))}
                placeholder={`Pilihan ${String.fromCharCode(65 + index)}`}
              />
              {option.is_correct && (
                <span style={{ color: '#4CAF50', fontSize: 12, whiteSpace: 'nowrap' }}>✓ Benar</span>
              )}
            </div>
          ))}

          {isEdit && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 12 }}>
              <input
                id="triviaQuestionActive"
                type="checkbox"
                checked={isActive}
                onChange={(event) => setIsActive(event.target.checked)}
              />
              <label htmlFor="triviaQuestionActive" style={styles.label}>
                Soal aktif dan dapat dipilih secara acak
              </label>
            </div>
          )}
        </div>

        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}>
            <Check size={14} /> {saving ? 'Menyimpan...' : 'Simpan Soal'}
          </button>
        </div>
      </div>
    </div>
  );
};

// -----------------------------------------------------------------------------
// MODAL EDIT KATEGORI (LIMA KATEGORI TETAP)
// -----------------------------------------------------------------------------
type CategoryModalProps = {
  category: TriviaCategory;
  onClose: () => void;
  onSave: () => Promise<void> | void;
};

const CategoryModal: React.FC<CategoryModalProps> = ({ category, onClose, onSave }) => {
  const [form, setForm] = useState({
    name: category.name,
    description: category.description,
    display_order: category.display_order,
  });
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const handleSave = async () => {
    if (!form.name.trim()) {
      setMsg('Nama kategori wajib diisi.');
      return;
    }

    setSaving(true);
    setMsg('');
    try {
      await adminUpdateTriviaCategory(category.id, {
        name: form.name.trim(),
        description: form.description.trim(),
        display_order: form.display_order,
      });
      await onSave();
    } catch (error) {
      setMsg(getApiError(error, 'Gagal memperbarui kategori.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div style={styles.overlay}>
      <div style={styles.modal}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>Edit Kategori Trivia</h3>
          <button onClick={onClose} style={styles.closeBtn} aria-label="Tutup">
            <X size={18} />
          </button>
        </div>

        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}
          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Nama Kategori *</label>
              <input
                style={styles.input}
                value={form.name}
                onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
              />
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Urutan Tampil</label>
              <input
                style={styles.input}
                type="number"
                min={1}
                value={form.display_order}
                onChange={(event) => setForm((current) => ({
                  ...current,
                  display_order: Number(event.target.value),
                }))}
              />
            </div>
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Deskripsi</label>
            <input
              style={styles.input}
              value={form.description}
              onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
              placeholder="Deskripsi kategori..."
            />
          </div>
          <div style={{
            padding: 12,
            borderRadius: 8,
            background: 'rgba(76,175,80,0.10)',
            border: '1px solid rgba(76,175,80,0.35)',
            fontSize: '0.8rem',
            lineHeight: 1.6,
          }}>
            Lima kategori Trivia merupakan struktur tetap. Setiap sesi memainkan {SESSION_QUESTION_COUNT} soal acak dari bank soal aktif kategori ini. Jumlah soal bank tidak dibatasi.
          </div>
        </div>

        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}>
            <Check size={14} /> {saving ? 'Menyimpan...' : 'Simpan Kategori'}
          </button>
        </div>
      </div>
    </div>
  );
};

// -----------------------------------------------------------------------------
// HALAMAN UTAMA TRIVIA
// -----------------------------------------------------------------------------
const TriviaPage: React.FC = () => {
  const [categories, setCategories] = useState<TriviaCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState('');
  const [selected, setSelected] = useState<TriviaCategory | null>(null);
  const [questions, setQuestions] = useState<TriviaQuestion[]>([]);
  const [questionLoading, setQuestionLoading] = useState(false);
  const [showCategoryModal, setShowCategoryModal] = useState(false);
  const [editCategory, setEditCategory] = useState<TriviaCategory | null>(null);
  const [showQuestionModal, setShowQuestionModal] = useState(false);
  const [editQuestion, setEditQuestion] = useState<TriviaQuestion | null>(null);

  const fetchCategories = async () => {
    setLoading(true);
    setPageError('');
    try {
      const response = await adminGetTriviaCategories();
      setCategories(response.data || []);
    } catch (error) {
      setPageError(getApiError(error, 'Gagal mengambil kategori Trivia.'));
    } finally {
      setLoading(false);
    }
  };

  const loadQuestions = async (category: TriviaCategory) => {
    setSelected(category);
    setQuestionLoading(true);
    setPageError('');
    try {
      const response = await adminGetTriviaQuestions(category.id);
      setQuestions(response.data || []);
    } catch (error) {
      setQuestions([]);
      setPageError(getApiError(error, 'Gagal mengambil bank soal kategori.'));
    } finally {
      setQuestionLoading(false);
    }
  };

  const handleDeactivateQuestion = async (questionID: number) => {
    if (!window.confirm('Nonaktifkan soal ini? Soal tetap tersimpan di bank, tetapi tidak dipilih untuk sesi baru.')) {
      return;
    }
    try {
      await adminDeleteTriviaQuestion(questionID);
      if (selected) await loadQuestions(selected);
    } catch (error) {
      setPageError(getApiError(error, 'Gagal menonaktifkan soal Trivia.'));
    }
  };

  const handleCategorySaved = async () => {
    setShowCategoryModal(false);
    setEditCategory(null);
    await fetchCategories();
    if (selected) {
      const refreshed = await adminGetTriviaCategories();
      const updatedSelected = (refreshed.data || []).find(
        (category: TriviaCategory) => category.id === selected.id,
      );
      if (updatedSelected) setSelected(updatedSelected);
    }
  };

  useEffect(() => {
    let mounted = true;
    adminGetTriviaCategories()
      .then((response) => {
        if (mounted) setCategories(response.data || []);
      })
      .catch((error: unknown) => {
        if (mounted) setPageError(getApiError(error, 'Gagal mengambil kategori Trivia.'));
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, []);

  const activeQuestionCount = questions.filter((question) => question.is_active).length;

  return (
    <div className="dashboard-container">
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><HelpCircle size={24} /></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Kelola Trivia Budaya</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>
              {categories.length} kategori tetap • bank soal tidak dibatasi
            </p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={() => void fetchCategories()} style={styles.btnSecondary}>
            <RefreshCw size={14} /> Refresh
          </button>
        </div>
      </div>

      {pageError && <div className="wood-panel" style={{ color: '#ff9b9b', marginBottom: 12 }}>{pageError}</div>}

      <div style={{ display: 'grid', gridTemplateColumns: '300px 1fr', gap: 20, alignItems: 'start' }}>
        <div className="wood-panel">
          <div className="panel-head">
            <h3>Kategori</h3>
            <span style={{ fontSize: '0.75rem', opacity: 0.5 }}>{categories.length} total</span>
          </div>

          {loading ? (
            <p style={{ opacity: 0.5, fontSize: '0.8rem' }}>Memuat kategori...</p>
          ) : categories.length === 0 ? (
            <p style={{ opacity: 0.5, fontSize: '0.8rem' }}>Belum ada kategori di database.</p>
          ) : categories.map((category) => (
            <div key={category.id} style={{ marginBottom: 6 }}>
              <div
                onClick={() => void loadQuestions(category)}
                style={{
                  padding: '10px 12px',
                  borderRadius: 8,
                  cursor: 'pointer',
                  background: selected?.id === category.id ? '#4D7023' : 'rgba(0,0,0,0.2)',
                  border: selected?.id === category.id ? '1px solid #D4AF37' : '1px solid transparent',
                  transition: '0.2s',
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                }}
              >
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontWeight: 'bold', fontSize: '0.85rem' }}>{category.name}</div>
                  <div style={{ fontSize: '0.7rem', opacity: 0.65 }}>
                    {SESSION_QUESTION_COUNT} soal per sesi • {category.is_active ? 'Aktif' : 'Nonaktif'}
                  </div>
                </div>
                <button
                  onClick={(event) => {
                    event.stopPropagation();
                    setEditCategory(category);
                    setShowCategoryModal(true);
                  }}
                  style={{ background: 'none', border: 'none', color: '#D4AF37', cursor: 'pointer' }}
                  title="Edit kategori"
                >
                  <Edit2 size={13} />
                </button>
              </div>
            </div>
          ))}
        </div>

        <div className="wood-panel">
          {!selected ? (
            <div style={{ textAlign: 'center', padding: 60, opacity: 0.4 }}>
              <HelpCircle size={48} style={{ margin: '0 auto 12px', display: 'block' }} />
              <p>Pilih kategori untuk mengelola bank soal.</p>
            </div>
          ) : (
            <>
              <div className="panel-head">
                <div>
                  <h3 style={{ marginBottom: 3 }}>{selected.name}</h3>
                  <div style={{ fontSize: '0.72rem', opacity: 0.7 }}>
                    {activeQuestionCount} soal aktif / {questions.length} total bank • {SESSION_QUESTION_COUNT} soal dipilih per sesi
                  </div>
                </div>
                <button
                  onClick={() => {
                    setEditQuestion(null);
                    setShowQuestionModal(true);
                  }}
                  style={styles.btnPrimary}
                >
                  <Plus size={13} /> Tambah Soal
                </button>
              </div>

              {questionLoading ? (
                <p style={{ opacity: 0.5, textAlign: 'center', padding: 20 }}>Memuat bank soal...</p>
              ) : questions.length === 0 ? (
                <p style={{ opacity: 0.4, textAlign: 'center', padding: 20 }}>
                  Belum ada soal. Tambahkan soal pertama untuk kategori ini.
                </p>
              ) : questions.map((question, index) => (
                <div key={question.id} style={{
                  marginBottom: 10,
                  background: 'rgba(0,0,0,0.2)',
                  borderRadius: 8,
                  overflow: 'hidden',
                  border: '1px solid rgba(212,175,55,0.1)',
                }}>
                  <div style={{ padding: '12px 16px', display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 8 }}>
                    <div style={{ flex: 1 }}>
                      <span style={{ fontSize: '0.7rem', color: '#D4AF37', marginRight: 8 }}>#{index + 1}</span>
                      <span style={{ fontSize: '0.85rem' }}>{question.question}</span>
                      <span className={`badge ${question.is_active ? 'green' : 'orange'}`} style={{ marginLeft: 8, fontSize: 9 }}>
                        {question.is_active ? 'Aktif' : 'Nonaktif'}
                      </span>
                    </div>
                    <div style={{ display: 'flex', gap: 4, flexShrink: 0 }}>
                      <button
                        onClick={() => {
                          setEditQuestion(question);
                          setShowQuestionModal(true);
                        }}
                        style={{ background: 'none', border: 'none', color: '#D4AF37', cursor: 'pointer' }}
                        title="Edit soal"
                      >
                        <Edit2 size={14} />
                      </button>
                      {question.is_active && (
                        <button
                          onClick={() => void handleDeactivateQuestion(question.id)}
                          style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer' }}
                          title="Nonaktifkan soal"
                        >
                          <Trash2 size={14} />
                        </button>
                      )}
                    </div>
                  </div>

                  <div style={{ padding: '0 16px 12px', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 6 }}>
                    {question.options?.map((option, optionIndex) => (
                      <div
                        key={option.id ?? optionIndex}
                        style={{
                          padding: '5px 10px',
                          borderRadius: 5,
                          fontSize: '0.78rem',
                          background: option.is_correct ? 'rgba(76,175,80,0.15)' : 'rgba(0,0,0,0.2)',
                          border: option.is_correct ? '1px solid #4CAF50' : '1px solid rgba(255,255,255,0.05)',
                        }}
                      >
                        {String.fromCharCode(65 + optionIndex)}. {option.option_text}
                        {option.is_correct && <span style={{ color: '#4CAF50', marginLeft: 4 }}>✓</span>}
                      </div>
                    ))}
                  </div>
                  <div style={{
                    padding: '0 16px 12px',
                    fontSize: '0.75rem',
                    opacity: 0.7,
                    borderLeft: '3px solid #D4AF37',
                    marginLeft: 16,
                    marginRight: 16,
                    paddingLeft: 10,
                  }}>
                    💡 {question.explanation}
                  </div>
                </div>
              ))}
            </>
          )}
        </div>
      </div>

      {showCategoryModal && editCategory && (
        <CategoryModal
          category={editCategory}
          onClose={() => {
            setShowCategoryModal(false);
            setEditCategory(null);
          }}
          onSave={handleCategorySaved}
        />
      )}

      {showQuestionModal && selected && (
        <QuestionModal
          key={`trivia-question-${selected.id}-${editQuestion?.id ?? 'new'}`}
          catId={selected.id}
          question={editQuestion}
          onClose={() => {
            setShowQuestionModal(false);
            setEditQuestion(null);
          }}
          onSave={async () => {
            setShowQuestionModal(false);
            setEditQuestion(null);
            await loadQuestions(selected);
          }}
        />
      )}
    </div>
  );
};

export default TriviaPage;
