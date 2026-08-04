import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, Plus, ClipboardList, Calendar, BarChart3, AlertTriangle,
} from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { StatusBadge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './EquipmentDetailPage.module.css'

interface CertificateDto {
  issuingLab: string
  accreditationRef: string | null
  fileRef: string | null
  validFrom: string
  validUntil: string
}

interface CalibrationRecordDto {
  id: string
  calibrationDate: string
  result: string
  technicianId: string | null
  procedureRef: string | null
  notes: string | null
  environmentalConditions: string | null
  nextDueDate: string | null
  certificate: CertificateDto | null
}

interface ScheduleDto {
  id: string
  intervalDays: number
  leadTimeDays: number
  labType: string
  procedureRef: string | null
  nextDueDate: string
}

interface GaugeStudyDto {
  id: string
  characteristicId: string | null
  studyDate: string
  totalGRRPct: number
  repeatabilityPct: number
  reproducibilityPct: number
  partVariationPct: number | null
  ndc: number | null
  result: string
}

interface ImpactAssessmentDto {
  id: string
  equipmentId: string
  failedCalibrationId: string
  affectedFrom: string
  affectedTo: string
  affectedInspectionCount: number
  status: string
  reviewedBy: string | null
  notes: string | null
}

interface EquipmentDetail {
  id: string
  code: string
  name: string
  type: string | null
  manufacturer: string | null
  model: string | null
  serialNumber: string | null
  status: string
  location: string | null
  department: string | null
  area: string | null
  custodianId: string | null
  schedule: ScheduleDto | null
  calibrations: CalibrationRecordDto[]
  gaugeStudies: GaugeStudyDto[]
  impactAssessments: ImpactAssessmentDto[]
  createdAt: string
}

type Tab = 'calibrations' | 'schedule' | 'gaugerr' | 'impact'

const resultOptions = [
  { value: 'Pass', label: 'Pass' },
  { value: 'PassWithAdjustment', label: 'Pass with Adjustment' },
  { value: 'Fail', label: 'Fail' },
  { value: 'Limited', label: 'Limited' },
]

export function EquipmentDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('calibrations')
  const [actionError, setActionError] = useState('')

  const [showCalForm, setShowCalForm] = useState(false)
  const [calForm, setCalForm] = useState({
    calibrationDate: '', result: 'Pass', technicianId: '',
    procedureRef: '', notes: '', environmentalConditions: '',
  })

  const [showCertForm, setShowCertForm] = useState<string | null>(null)
  const [certForm, setCertForm] = useState({
    issuingLab: '', accreditationRef: '', fileRef: '',
    validFrom: '', validUntil: '',
  })

  const [showScheduleForm, setShowScheduleForm] = useState(false)
  const [scheduleForm, setScheduleForm] = useState({
    intervalDays: '', leadTimeDays: '', labType: '',
    procedureRef: '', nextDueDate: '',
  })

  const [showGaugeForm, setShowGaugeForm] = useState(false)
  const [gaugeForm, setGaugeForm] = useState({
    characteristicId: '', studyDate: '', totalGRRPct: '',
    repeatabilityPct: '', reproducibilityPct: '',
    partVariationPct: '', ndc: '',
  })

  const { data: equipment, isLoading } = useQuery<EquipmentDetail>({
    queryKey: ['equipment', id],
    queryFn: async () => {
      const res = await apiClient.get(`/api/v1/equipment/${id}`)
      return res.data
    },
    enabled: Boolean(id),
  })

  const handleError = (err: unknown) => {
    const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
    setActionError(
      axiosErr.response?.data?.detail ??
        axiosErr.response?.data?.error ??
        'An error occurred.'
    )
  }

  const invalidate = () => {
    setActionError('')
    queryClient.invalidateQueries({ queryKey: ['equipment', id] })
    queryClient.invalidateQueries({ queryKey: ['equipment'] })
  }

  const calMutation = useMutation({
    mutationFn: () => apiClient.post(`/api/v1/equipment/${id}/calibrations`, {
      calibrationDate: calForm.calibrationDate,
      result: calForm.result,
      technicianId: calForm.technicianId || null,
      procedureRef: calForm.procedureRef || null,
      notes: calForm.notes || null,
      environmentalConditions: calForm.environmentalConditions || null,
    }),
    onSuccess: () => {
      invalidate(); setShowCalForm(false)
      setCalForm({ calibrationDate: '', result: 'Pass', technicianId: '', procedureRef: '', notes: '', environmentalConditions: '' })
    },
    onError: handleError,
  })

  const certMutation = useMutation({
    mutationFn: (calId: string) => apiClient.post(`/api/v1/equipment/${id}/calibrations/${calId}/certificate`, {
      issuingLab: certForm.issuingLab,
      accreditationRef: certForm.accreditationRef || null,
      fileRef: certForm.fileRef || null,
      validFrom: certForm.validFrom,
      validUntil: certForm.validUntil,
    }),
    onSuccess: () => {
      invalidate(); setShowCertForm(null)
      setCertForm({ issuingLab: '', accreditationRef: '', fileRef: '', validFrom: '', validUntil: '' })
    },
    onError: handleError,
  })

  const scheduleMutation = useMutation({
    mutationFn: () => apiClient.put(`/api/v1/equipment/${id}/schedule`, {
      intervalDays: Number(scheduleForm.intervalDays),
      leadTimeDays: Number(scheduleForm.leadTimeDays),
      labType: scheduleForm.labType,
      procedureRef: scheduleForm.procedureRef || null,
      nextDueDate: scheduleForm.nextDueDate,
    }),
    onSuccess: () => {
      invalidate(); setShowScheduleForm(false)
      setScheduleForm({ intervalDays: '', leadTimeDays: '', labType: '', procedureRef: '', nextDueDate: '' })
    },
    onError: handleError,
  })

  const gaugeMutation = useMutation({
    mutationFn: () => apiClient.post(`/api/v1/equipment/${id}/gauge-rr`, {
      characteristicId: gaugeForm.characteristicId || null,
      studyDate: gaugeForm.studyDate,
      totalGRRPct: Number(gaugeForm.totalGRRPct),
      repeatabilityPct: Number(gaugeForm.repeatabilityPct),
      reproducibilityPct: Number(gaugeForm.reproducibilityPct),
      partVariationPct: gaugeForm.partVariationPct ? Number(gaugeForm.partVariationPct) : null,
      ndc: gaugeForm.ndc ? Number(gaugeForm.ndc) : null,
    }),
    onSuccess: () => {
      invalidate(); setShowGaugeForm(false)
      setGaugeForm({ characteristicId: '', studyDate: '', totalGRRPct: '', repeatabilityPct: '', reproducibilityPct: '', partVariationPct: '', ndc: '' })
    },
    onError: handleError,
  })

  const reviewMutation = useMutation({
    mutationFn: ({ assessmentId, action, notes }: { assessmentId: string; action: string; notes?: string }) =>
      apiClient.put(`/api/v1/calibration/impact-assessment/${assessmentId}`, { action, notes }),
    onSuccess: invalidate,
    onError: handleError,
  })

  if (isLoading || !equipment) {
    return <div className={styles.page}>Loading...</div>
  }

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <button
          type="button"
          className={styles.backButton}
          onClick={() => navigate('/calibration')}
        >
          <ArrowLeft size={16} />
        </button>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{equipment.code} — {equipment.name}</h1>
            <StatusBadge status={equipment.status} />
          </div>
          <div className={styles.meta}>
            <span><span className={styles.metaLabel}>Type: </span>{equipment.type ?? '—'}</span>
            <span><span className={styles.metaLabel}>Location: </span>{equipment.location ?? '—'}</span>
            <span><span className={styles.metaLabel}>Created: </span>{new Date(equipment.createdAt).toLocaleDateString()}</span>
          </div>
        </div>
      </div>

      {actionError && <div className={styles.errorBanner}>{actionError}</div>}

      <div className={styles.infoGrid}>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Manufacturer</span>
          <span className={styles.infoValue}>{equipment.manufacturer ?? '—'}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Model</span>
          <span className={styles.infoValue}>{equipment.model ?? '—'}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Serial Number</span>
          <span className={styles.infoValue}>{equipment.serialNumber ?? '—'}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Department</span>
          <span className={styles.infoValue}>{equipment.department ?? '—'}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Area</span>
          <span className={styles.infoValue}>{equipment.area ?? '—'}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Calibrations</span>
          <span className={styles.infoValue}>{equipment.calibrations.length}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Gauge Studies</span>
          <span className={styles.infoValue}>{equipment.gaugeStudies.length}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Next Due</span>
          <span className={styles.infoValue}>
            {equipment.schedule?.nextDueDate
              ? new Date(equipment.schedule.nextDueDate).toLocaleDateString()
              : '—'}
          </span>
        </div>
      </div>

      <div className={styles.tabs}>
        <button className={`${styles.tab} ${tab === 'calibrations' ? styles.tabActive : ''}`} onClick={() => setTab('calibrations')}>
          <ClipboardList size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Calibrations ({equipment.calibrations.length})
        </button>
        <button className={`${styles.tab} ${tab === 'schedule' ? styles.tabActive : ''}`} onClick={() => setTab('schedule')}>
          <Calendar size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Schedule
        </button>
        <button className={`${styles.tab} ${tab === 'gaugerr' ? styles.tabActive : ''}`} onClick={() => setTab('gaugerr')}>
          <BarChart3 size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Gauge R&R ({equipment.gaugeStudies.length})
        </button>
        <button className={`${styles.tab} ${tab === 'impact' ? styles.tabActive : ''}`} onClick={() => setTab('impact')}>
          <AlertTriangle size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Impact Assessments ({equipment.impactAssessments.length})
        </button>
      </div>

      {tab === 'calibrations' && (
        <div className={styles.tabContent}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Calibration Records</h2>
            {!showCalForm && (
              <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowCalForm(true)}>
                Record Calibration
              </Button>
            )}
          </div>

          {showCalForm && (
            <div className={styles.inlineForm}>
              <h3 className={styles.formTitle}>Record Calibration</h3>
              <div className={styles.formGrid}>
                <Input
                  label="Calibration Date"
                  type="date"
                  value={calForm.calibrationDate}
                  onChange={(e) => setCalForm(f => ({ ...f, calibrationDate: e.target.value }))}
                  required
                />
                <Select
                  label="Result"
                  options={resultOptions}
                  value={calForm.result}
                  onChange={(e) => setCalForm(f => ({ ...f, result: e.target.value }))}
                />
                <Input
                  label="Procedure Ref"
                  value={calForm.procedureRef}
                  onChange={(e) => setCalForm(f => ({ ...f, procedureRef: e.target.value }))}
                  placeholder="e.g., SOP-CAL-001"
                />
              </div>
              <div className={styles.formGrid2}>
                <Input
                  label="Notes"
                  value={calForm.notes}
                  onChange={(e) => setCalForm(f => ({ ...f, notes: e.target.value }))}
                  placeholder="Calibration notes..."
                />
                <Input
                  label="Environmental Conditions"
                  value={calForm.environmentalConditions}
                  onChange={(e) => setCalForm(f => ({ ...f, environmentalConditions: e.target.value }))}
                  placeholder="e.g., 22°C, 45% RH"
                />
              </div>
              <div className={styles.formActions}>
                <Button size="sm" variant="ghost" onClick={() => setShowCalForm(false)}>Cancel</Button>
                <Button size="sm" onClick={() => calMutation.mutate()}
                  disabled={!calForm.calibrationDate || calMutation.isPending}>
                  {calMutation.isPending ? 'Recording...' : 'Record'}
                </Button>
              </div>
            </div>
          )}

          {equipment.calibrations.length === 0 ? (
            <p className={styles.empty}>No calibration records yet.</p>
          ) : (
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Result</th>
                  <th>Procedure</th>
                  <th>Next Due</th>
                  <th>Certificate</th>
                  <th>Notes</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {equipment.calibrations.map((cal) => (
                  <tr key={cal.id}>
                    <td>{new Date(cal.calibrationDate).toLocaleDateString()}</td>
                    <td><StatusBadge status={cal.result} /></td>
                    <td>{cal.procedureRef ?? '—'}</td>
                    <td>{cal.nextDueDate ? new Date(cal.nextDueDate).toLocaleDateString() : '—'}</td>
                    <td>{cal.certificate ? cal.certificate.issuingLab : '—'}</td>
                    <td>{cal.notes ? (cal.notes.length > 40 ? `${cal.notes.substring(0, 40)}...` : cal.notes) : '—'}</td>
                    <td>
                      {!cal.certificate && showCertForm !== cal.id && (
                        <button
                          type="button"
                          className={styles.actionBtn}
                          onClick={() => setShowCertForm(cal.id)}
                        >
                          Attach Cert
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {showCertForm && (
            <div className={styles.inlineForm}>
              <h3 className={styles.formTitle}>Attach Certificate</h3>
              <div className={styles.formGrid}>
                <Input
                  label="Issuing Lab"
                  value={certForm.issuingLab}
                  onChange={(e) => setCertForm(f => ({ ...f, issuingLab: e.target.value }))}
                  placeholder="Laboratory name" required
                />
                <Input
                  label="Valid From"
                  type="date"
                  value={certForm.validFrom}
                  onChange={(e) => setCertForm(f => ({ ...f, validFrom: e.target.value }))}
                  required
                />
                <Input
                  label="Valid Until"
                  type="date"
                  value={certForm.validUntil}
                  onChange={(e) => setCertForm(f => ({ ...f, validUntil: e.target.value }))}
                  required
                />
              </div>
              <div className={styles.formGrid2}>
                <Input
                  label="Accreditation Ref"
                  value={certForm.accreditationRef}
                  onChange={(e) => setCertForm(f => ({ ...f, accreditationRef: e.target.value }))}
                  placeholder="e.g., ISO/IEC 17025"
                />
                <Input
                  label="File Reference"
                  value={certForm.fileRef}
                  onChange={(e) => setCertForm(f => ({ ...f, fileRef: e.target.value }))}
                  placeholder="Certificate file ref..."
                />
              </div>
              <div className={styles.formActions}>
                <Button size="sm" variant="ghost" onClick={() => setShowCertForm(null)}>Cancel</Button>
                <Button size="sm" onClick={() => certMutation.mutate(showCertForm)}
                  disabled={!certForm.issuingLab.trim() || !certForm.validFrom || !certForm.validUntil || certMutation.isPending}>
                  {certMutation.isPending ? 'Attaching...' : 'Attach Certificate'}
                </Button>
              </div>
            </div>
          )}
        </div>
      )}

      {tab === 'schedule' && (
        <div className={styles.tabContent}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Calibration Schedule</h2>
            {!showScheduleForm && (
              <Button
                size="sm"
                icon={<Plus size={14} />}
                onClick={() => {
                  if (equipment.schedule) {
                    setScheduleForm({
                      intervalDays: String(equipment.schedule.intervalDays),
                      leadTimeDays: String(equipment.schedule.leadTimeDays),
                      labType: equipment.schedule.labType,
                      procedureRef: equipment.schedule.procedureRef ?? '',
                      nextDueDate: equipment.schedule.nextDueDate.split('T')[0],
                    })
                  }
                  setShowScheduleForm(true)
                }}
              >
                {equipment.schedule ? 'Update Schedule' : 'Set Schedule'}
              </Button>
            )}
          </div>

          {equipment.schedule ? (
            <div className={styles.scheduleCard}>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Interval</span>
                <span className={styles.infoValue}>{equipment.schedule.intervalDays} days</span>
              </div>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Lead Time</span>
                <span className={styles.infoValue}>{equipment.schedule.leadTimeDays} days</span>
              </div>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Lab Type</span>
                <span className={styles.infoValue}>{equipment.schedule.labType}</span>
              </div>
              <div className={styles.infoItem}>
                <span className={styles.infoLabel}>Next Due Date</span>
                <span className={styles.infoValue}>{new Date(equipment.schedule.nextDueDate).toLocaleDateString()}</span>
              </div>
              {equipment.schedule.procedureRef && (
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Procedure</span>
                  <span className={styles.infoValue}>{equipment.schedule.procedureRef}</span>
                </div>
              )}
            </div>
          ) : (
            <p className={styles.empty}>No calibration schedule configured.</p>
          )}

          {showScheduleForm && (
            <div className={styles.inlineForm}>
              <h3 className={styles.formTitle}>{equipment.schedule ? 'Update' : 'Set'} Schedule</h3>
              <div className={styles.formGrid}>
                <Input
                  label="Interval (days)"
                  type="number"
                  value={scheduleForm.intervalDays}
                  onChange={(e) => setScheduleForm(f => ({ ...f, intervalDays: e.target.value }))}
                  min={1} required
                />
                <Input
                  label="Lead Time (days)"
                  type="number"
                  value={scheduleForm.leadTimeDays}
                  onChange={(e) => setScheduleForm(f => ({ ...f, leadTimeDays: e.target.value }))}
                  min={0} required
                />
                <Input
                  label="Lab Type"
                  value={scheduleForm.labType}
                  onChange={(e) => setScheduleForm(f => ({ ...f, labType: e.target.value }))}
                  placeholder="e.g., Internal, External" required
                />
              </div>
              <div className={styles.formGrid2}>
                <Input
                  label="Next Due Date"
                  type="date"
                  value={scheduleForm.nextDueDate}
                  onChange={(e) => setScheduleForm(f => ({ ...f, nextDueDate: e.target.value }))}
                  required
                />
                <Input
                  label="Procedure Ref"
                  value={scheduleForm.procedureRef}
                  onChange={(e) => setScheduleForm(f => ({ ...f, procedureRef: e.target.value }))}
                  placeholder="e.g., SOP-CAL-001"
                />
              </div>
              <div className={styles.formActions}>
                <Button size="sm" variant="ghost" onClick={() => setShowScheduleForm(false)}>Cancel</Button>
                <Button size="sm" onClick={() => scheduleMutation.mutate()}
                  disabled={!scheduleForm.intervalDays || !scheduleForm.leadTimeDays || !scheduleForm.labType.trim() || !scheduleForm.nextDueDate || scheduleMutation.isPending}>
                  {scheduleMutation.isPending ? 'Saving...' : 'Save Schedule'}
                </Button>
              </div>
            </div>
          )}
        </div>
      )}

      {tab === 'gaugerr' && (
        <div className={styles.tabContent}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Gauge R&R Studies</h2>
            {!showGaugeForm && (
              <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowGaugeForm(true)}>
                Record Study
              </Button>
            )}
          </div>

          {showGaugeForm && (
            <div className={styles.inlineForm}>
              <h3 className={styles.formTitle}>Record Gauge R&R Study</h3>
              <div className={styles.formGrid}>
                <Input
                  label="Study Date"
                  type="date"
                  value={gaugeForm.studyDate}
                  onChange={(e) => setGaugeForm(f => ({ ...f, studyDate: e.target.value }))}
                  required
                />
                <Input
                  label="Total GRR %"
                  type="number"
                  value={gaugeForm.totalGRRPct}
                  onChange={(e) => setGaugeForm(f => ({ ...f, totalGRRPct: e.target.value }))}
                  min={0} max={100} step={0.01} required
                />
                <Input
                  label="Repeatability %"
                  type="number"
                  value={gaugeForm.repeatabilityPct}
                  onChange={(e) => setGaugeForm(f => ({ ...f, repeatabilityPct: e.target.value }))}
                  min={0} max={100} step={0.01} required
                />
              </div>
              <div className={styles.formGrid}>
                <Input
                  label="Reproducibility %"
                  type="number"
                  value={gaugeForm.reproducibilityPct}
                  onChange={(e) => setGaugeForm(f => ({ ...f, reproducibilityPct: e.target.value }))}
                  min={0} max={100} step={0.01} required
                />
                <Input
                  label="Part Variation %"
                  type="number"
                  value={gaugeForm.partVariationPct}
                  onChange={(e) => setGaugeForm(f => ({ ...f, partVariationPct: e.target.value }))}
                  min={0} max={100} step={0.01}
                />
                <Input
                  label="NDC"
                  type="number"
                  value={gaugeForm.ndc}
                  onChange={(e) => setGaugeForm(f => ({ ...f, ndc: e.target.value }))}
                  min={0}
                />
              </div>
              <div className={styles.formActions}>
                <Button size="sm" variant="ghost" onClick={() => setShowGaugeForm(false)}>Cancel</Button>
                <Button size="sm" onClick={() => gaugeMutation.mutate()}
                  disabled={!gaugeForm.studyDate || !gaugeForm.totalGRRPct || !gaugeForm.repeatabilityPct || !gaugeForm.reproducibilityPct || gaugeMutation.isPending}>
                  {gaugeMutation.isPending ? 'Recording...' : 'Record Study'}
                </Button>
              </div>
            </div>
          )}

          {equipment.gaugeStudies.length === 0 ? (
            <p className={styles.empty}>No Gauge R&R studies recorded.</p>
          ) : (
            equipment.gaugeStudies.map((gs) => (
              <div key={gs.id} className={styles.card}>
                <div className={styles.cardHeader}>
                  <h3 className={styles.cardTitle}>
                    Study — {new Date(gs.studyDate).toLocaleDateString()}
                  </h3>
                  <StatusBadge status={gs.result} />
                </div>
                <div className={styles.cardGrid}>
                  <div>
                    <div className={styles.cardLabel}>Total GRR</div>
                    <div className={styles.cardValue}>{gs.totalGRRPct.toFixed(2)}%</div>
                  </div>
                  <div>
                    <div className={styles.cardLabel}>Repeatability</div>
                    <div className={styles.cardValue}>{gs.repeatabilityPct.toFixed(2)}%</div>
                  </div>
                  <div>
                    <div className={styles.cardLabel}>Reproducibility</div>
                    <div className={styles.cardValue}>{gs.reproducibilityPct.toFixed(2)}%</div>
                  </div>
                  {gs.partVariationPct !== null && (
                    <div>
                      <div className={styles.cardLabel}>Part Variation</div>
                      <div className={styles.cardValue}>{gs.partVariationPct.toFixed(2)}%</div>
                    </div>
                  )}
                  {gs.ndc !== null && (
                    <div>
                      <div className={styles.cardLabel}>NDC</div>
                      <div className={styles.cardValue}>{gs.ndc}</div>
                    </div>
                  )}
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {tab === 'impact' && (
        <div className={styles.tabContent}>
          <div className={styles.sectionHeader}>
            <h2 className={styles.sectionTitle}>Impact Assessments</h2>
          </div>

          {equipment.impactAssessments.length === 0 ? (
            <p className={styles.empty}>No impact assessments. These are created automatically when a calibration fails.</p>
          ) : (
            equipment.impactAssessments.map((ia) => (
              <div key={ia.id} className={styles.card}>
                <div className={styles.cardHeader}>
                  <h3 className={styles.cardTitle}>
                    Impact Assessment — {new Date(ia.affectedFrom).toLocaleDateString()} to {new Date(ia.affectedTo).toLocaleDateString()}
                  </h3>
                  <StatusBadge status={ia.status} />
                </div>
                <div className={styles.cardGrid}>
                  <div>
                    <div className={styles.cardLabel}>Affected Inspections</div>
                    <div className={styles.cardValue}>{ia.affectedInspectionCount}</div>
                  </div>
                  <div>
                    <div className={styles.cardLabel}>Affected Period</div>
                    <div className={styles.cardValue}>
                      {new Date(ia.affectedFrom).toLocaleDateString()} — {new Date(ia.affectedTo).toLocaleDateString()}
                    </div>
                  </div>
                  {ia.notes && (
                    <div>
                      <div className={styles.cardLabel}>Notes</div>
                      <div className={styles.cardValue}>{ia.notes}</div>
                    </div>
                  )}
                </div>
                <div className={styles.formActions} style={{ marginTop: 'var(--spacing-3)' }}>
                  {ia.status === 'Open' && (
                    <Button size="sm" onClick={() => reviewMutation.mutate({ assessmentId: ia.id, action: 'REVIEW' })}>
                      Start Review
                    </Button>
                  )}
                  {ia.status === 'UnderReview' && (
                    <Button size="sm" onClick={() => reviewMutation.mutate({ assessmentId: ia.id, action: 'CLOSE', notes: 'Review completed.' })}>
                      Close Assessment
                    </Button>
                  )}
                </div>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  )
}
