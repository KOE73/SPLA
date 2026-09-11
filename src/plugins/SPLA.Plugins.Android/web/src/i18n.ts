type Translate = (text: string, params?: Record<string, unknown>) => string;
let host: Translate | undefined;
export function useHostTranslator(fn?: Translate) { host = fn; }
const ru: Record<string, string> = {
  'Component folder': 'Папка компонента', 'Not installed': 'Не установлено', 'Installed': 'Установлено',
  'Verified': 'Хеш проверен', 'Not verified': 'Хеш не проверен', 'Experimental version': 'Экспериментальная версия',
  'Install': 'Установить', 'Install latest (experimental)': 'Установить latest (экспериментально)',
  'Stream': 'Видеопоток', 'Maximum size (0 = native)': 'Максимальный размер (0 = родной)', 'Maximum FPS': 'Максимальная частота кадров',
  'Bit rate': 'Битрейт', 'Settle time (ms)': 'Время стабильности (мс)', 'Settle timeout (ms)': 'Ожидание стабильности (мс)',
  'Keep screen awake': 'Не гасить экран', 'Screenshot after action': 'Снимок после действия', 'Idle disconnect (minutes)': 'Отключение по простою (минуты)',
  'ADB server port (0 = default)': 'Порт сервера ADB (0 = стандартный)', 'Refresh': 'Обновить',
  'downloading': 'скачивание', 'verifying': 'проверка хеша', 'extracting': 'распаковка', 'probing': 'проверка файлов', 'done': 'готово', 'failed': 'ошибка'
};
export function t(text: string): string {
  const translated = host?.(text);
  if (translated && translated !== text) return translated;
  const russian = document.documentElement.lang.startsWith('ru') || host?.('Settings') === 'Настройки';
  return russian ? ru[text] ?? text : text;
}
