import { useNavigate } from 'react-router-dom';
import type { SearchHit } from '../api';

export function ProductResults({ hits, onOpen, showStatus }: { hits: SearchHit[]; onOpen?: (hit: SearchHit) => void; showStatus?: boolean }) {
  const navigate = useNavigate();
  const open = (h: SearchHit) => {
    onOpen?.(h);
    navigate(`/products/${h.id}`);
  };
  return (
    <div className="panel">
      <div className="table-wrap">
        <table className="grid stack">
          <thead>
            <tr>
              <th>Product Code</th>
              <th>Product Name / Description</th>
              <th>Model / Application</th>
              <th>Supplier</th>
              <th>Rev.</th>
            </tr>
          </thead>
          <tbody>
            {hits.map((h) => (
              <tr key={h.id} className="clickable" onClick={() => open(h)}>
                <td className="col-code stack-title">
                  <a href={`/products/${h.id}`} onClick={(e) => { e.preventDefault(); }}>{h.productCode}</a>
                  {showStatus && h.status === 'archived' && <> <span className="badge badge-archived">Archived</span></>}
                </td>
                <td>
                  <div>{h.productName}</div>
                  {h.description && h.description !== h.productName && <div className="result-desc">{h.description}</div>}
                </td>
                <td data-label="Model / Application" className="stack-hide-empty">{h.model ?? ''}</td>
                <td data-label="Supplier" className="stack-hide-empty">{h.suppliers ?? ''}</td>
                <td data-label="Revision" className="nowrap">{h.currentRevision}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
