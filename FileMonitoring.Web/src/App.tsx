import { useEffect, useState } from 'react'
import axios from 'axios'
import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from 'recharts'
import './App.css'

interface ProcessedFile {
  id: string
  fileName: string
  company: string
  processingDate: string
  sequence: string
  status: 1 | 2 // 1=Recepcionado, 2=NaoRecepcionado
  createdAt: string
}

const API_URL = 'http://localhost:5122/api/files'

function App() {
  const [files, setFiles] = useState<ProcessedFile[]>([])
  const [stats, setStats] = useState({ received: 0, notReceived: 0 })
  const [loading, setLoading] = useState(false)

  const fetchData = async () => {
    try {
      const resFiles = await axios.get(API_URL)
      const resStats = await axios.get(`${API_URL}/summary`)

      setFiles(resFiles.data)
      setStats(resStats.data)
    } catch (err) {
      console.error(err)
    }
  }

  useEffect(() => {
    fetchData()
  }, [])

  const handleUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files || e.target.files.length === 0) return
    const formData = new FormData()
    formData.append('file', e.target.files[0])
    setLoading(true)
    try {
      await axios.post(`${API_URL}/upload`, formData)
      await fetchData()
    } catch (err) {
      alert('Error uploading file')
      console.error(err)
    }
    setLoading(false)
  }

  const data = [
    { name: 'Recepcionado', value: stats.received },
    { name: 'Não Recepcionado', value: stats.notReceived },
  ]

  const COLORS = ['#10b981', '#ef4444']

  return (
    <div className="container">
      <h1>Monitoramento de Arquivos</h1>

      <div className="upload-section">
        <label className="upload-btn">
          Upload Arquivo
          <input type="file" onChange={handleUpload} hidden />
        </label>
        {loading && <span className="loading">Processando...</span>}
      </div>

      <div className="dashboard">
        <div className="chart-container">
          <h3>Status dos Arquivos</h3>
          <div style={{ width: '100%', height: 300 }}>
            <ResponsiveContainer>
              <PieChart>
                <Pie
                  data={data}
                  cx="50%"
                  cy="50%"
                  outerRadius={80}
                  dataKey="value"
                  label
                >
                  {data.map((_, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>

        <div className="list-container">
          <h3>Arquivos Processados</h3>
          <div className="table-responsive">
            <table>
              <thead>
                <tr>
                  <th>Arquivo</th>
                  <th>Empresa</th>
                  <th>Data Proc.</th>
                  <th>Sequência</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {files.map(f => (
                  <tr key={f.id}>
                    <td>{f.fileName}</td>
                    <td>{f.company}</td>
                    <td>{new Date(f.processingDate).toLocaleDateString()}</td>
                    <td>{f.sequence}</td>
                    <td>
                      <span className={`badge ${f.status === 1 ? 'badge-success' : 'badge-error'}`}>
                        {f.status === 1 ? 'Recepcionado' : 'Não Recepcionado'}
                      </span>
                    </td>
                  </tr>
                ))}
                {files.length === 0 && (
                  <tr>
                    <td colSpan={5} style={{ textAlign: 'center' }}>Nenhum arquivo processado.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  )
}

export default App
