import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { reportsService } from '../../services'
import { formatDate, downloadBlob } from '../../utils/formatters'
import { FileText, FileSpreadsheet, Download, AlertCircle } from 'lucide-react'
import toast from 'react-hot-toast'

const reportTypes = [
  { value: 1, label: 'Monthly Budget' },
  { value: 2, label: 'Annual Summary' },
  { value: 3, label: 'Net Worth Statement' },
  { value: 4, label: 'Cash Flow Statement' },
  { value: 5, label: 'Debt Summary' },
  { value: 6, label: 'Investment Performance' },
  { value: 8, label: 'Spending Analysis' },
  { value: 9, label: 'Income Analysis' },
  { value: 10, label: 'Custom Date Range' },
]

export default function ReportsPage() {
  const now = new Date()
  const [form, setForm] = useState({
    reportType: 4,
    fromDate: new Date(now.getFullYear(), now.getMonth(), 1).toISOString().split('T')[0],
    toDate: now.toISOString().split('T')[0],
    outputFormat: 'pdf',
    includeCharts: true,
  })
  const [loading, setLoading] = useState(false)

  const handleGenerate = async () => {
    setLoading(true)
    try {
      const blob = await reportsService.generate({ ...form, fromDate: new Date(form.fromDate), toDate: new Date(form.toDate) })
      const ext = form.outputFormat === 'pdf' ? 'pdf' : 'xlsx'
      downloadBlob(blob, `BudgetPH_Report_${form.fromDate}_${form.toDate}.${ext}`)
      toast.success(`Report downloaded successfully!`)
    } catch {
      toast.error('Failed to generate report')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="card">
        <h3 className="text-dark-100 font-semibold text-lg mb-1">Generate Report</h3>
        <p className="text-dark-400 text-sm mb-6">Download detailed financial reports in PDF or Excel format</p>

        <div className="space-y-5">
          <div>
            <label className="label">Report Type</label>
            <select className="input" value={form.reportType} onChange={e => setForm(p => ({...p, reportType: +e.target.value}))}>
              {reportTypes.map(r => <option key={r.value} value={r.value}>{r.label}</option>)}
            </select>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="label">From Date</label>
              <input type="date" className="input" value={form.fromDate} onChange={e => setForm(p => ({...p, fromDate: e.target.value}))} />
            </div>
            <div>
              <label className="label">To Date</label>
              <input type="date" className="input" value={form.toDate} onChange={e => setForm(p => ({...p, toDate: e.target.value}))} />
            </div>
          </div>

          <div>
            <label className="label">Output Format</label>
            <div className="grid grid-cols-2 gap-3 mt-1">
              {[
                { value: 'pdf',   label: 'PDF Report',    icon: FileText,        desc: 'Best for printing & sharing' },
                { value: 'excel', label: 'Excel Workbook', icon: FileSpreadsheet, desc: 'Best for further analysis' },
              ].map(f => (
                <button
                  key={f.value}
                  type="button"
                  onClick={() => setForm(p => ({...p, outputFormat: f.value}))}
                  className={`p-4 rounded-xl border text-left transition-all ${form.outputFormat === f.value
                    ? 'border-primary-600 bg-primary-600/10 shadow-glow'
                    : 'border-dark-600 hover:border-dark-500 bg-dark-900/50'}`}
                >
                  <f.icon size={20} className={form.outputFormat === f.value ? 'text-primary-400' : 'text-dark-400'} />
                  <p className={`font-medium text-sm mt-2 ${form.outputFormat === f.value ? 'text-primary-300' : 'text-dark-200'}`}>{f.label}</p>
                  <p className="text-dark-500 text-xs mt-0.5">{f.desc}</p>
                </button>
              ))}
            </div>
          </div>

          <button
            onClick={handleGenerate}
            disabled={loading}
            className="btn-gold w-full justify-center py-3 text-base"
          >
            {loading ? (
              <span className="flex items-center gap-2">
                <span className="w-4 h-4 border-2 border-dark-900/50 border-t-dark-900 rounded-full animate-spin" />
                Generating…
              </span>
            ) : (
              <span className="flex items-center gap-2">
                <Download size={18} />
                Download {form.outputFormat === 'pdf' ? 'PDF' : 'Excel'} Report
              </span>
            )}
          </button>
        </div>
      </div>

      {/* Info */}
      <div className="card bg-primary-600/5 border-primary-600/20">
        <div className="flex gap-3">
          <AlertCircle size={16} className="text-primary-400 flex-shrink-0 mt-0.5" />
          <div className="text-sm text-dark-300 space-y-1">
            <p className="font-medium text-primary-300">What's included in reports</p>
            <ul className="text-dark-400 space-y-0.5 list-disc list-inside">
              <li>Income & expense summary with breakdowns</li>
              <li>Category-wise spending analysis</li>
              <li>Monthly trend comparisons</li>
              <li>Net worth change over period</li>
              <li>All transactions in the date range</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  )
}
