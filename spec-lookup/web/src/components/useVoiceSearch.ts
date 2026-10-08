import { useCallback, useEffect, useRef, useState } from 'react';

// Minimal typing for the Web Speech API (not in the standard DOM lib).
interface Recognition {
  lang: string; interimResults: boolean; maxAlternatives: number; continuous: boolean;
  start(): void; stop(): void; abort(): void;
  onresult: ((e: { results: ArrayLike<ArrayLike<{ transcript: string }> & { isFinal: boolean }> }) => void) | null;
  onerror: ((e: { error: string }) => void) | null;
  onend: (() => void) | null;
}
type RecognitionCtor = new () => Recognition;

function getCtor(): RecognitionCtor | null {
  const w = window as unknown as { SpeechRecognition?: RecognitionCtor; webkitSpeechRecognition?: RecognitionCtor };
  return w.SpeechRecognition ?? w.webkitSpeechRecognition ?? null;
}

/** Speech-to-text for the search box. Text is passed to the normal search; nothing else. */
export function useVoiceSearch(onText: (text: string, final: boolean) => void) {
  const [listening, setListening] = useState(false);
  const [message, setMessage] = useState('');
  const recRef = useRef<Recognition | null>(null);
  const cbRef = useRef(onText);
  cbRef.current = onText;

  useEffect(() => () => recRef.current?.abort(), []);

  const start = useCallback(() => {
    const Ctor = getCtor();
    if (!Ctor) {
      setMessage('Voice search is not supported on this browser.');
      return;
    }
    if (recRef.current) { recRef.current.stop(); return; }
    const rec = new Ctor();
    rec.lang = navigator.language || 'en-US';
    rec.interimResults = true;
    rec.maxAlternatives = 1;
    rec.continuous = false;
    rec.onresult = (e) => {
      let text = '';
      let final = false;
      for (let i = 0; i < e.results.length; i++) {
        text += e.results[i][0].transcript;
        if (e.results[i].isFinal) final = true;
      }
      cbRef.current(text, final);
    };
    rec.onerror = (e) => {
      if (e.error === 'not-allowed' || e.error === 'service-not-allowed') setMessage('Microphone access is blocked. Allow it in the browser settings.');
      else if (e.error === 'no-speech') setMessage('No speech detected. Try again.');
      else if (e.error === 'network') setMessage('Voice search needs an internet connection.');
      else if (e.error !== 'aborted') setMessage('Voice search failed. Type your search instead.');
    };
    rec.onend = () => { setListening(false); recRef.current = null; };
    recRef.current = rec;
    setMessage('Listening… say a product code or name.');
    setListening(true);
    try {
      rec.start();
    } catch {
      setListening(false);
      recRef.current = null;
      setMessage('Voice search could not start. Type your search instead.');
    }
  }, []);

  return { listening, message, setMessage, start };
}
